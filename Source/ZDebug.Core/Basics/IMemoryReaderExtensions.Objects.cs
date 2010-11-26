using System;
using ZDebug.Core.Objects;

namespace ZDebug.Core.Basics
{
    internal static partial class IMemoryReaderExtensions
    {
        private static ZProperty NextProperty_V1(this IMemoryReader reader, byte sizeByte, ZPropertyTable propertyTable)
        {
            var address = reader.Address - 1;
            var number = sizeByte % 32;
            var length = (sizeByte / 32) + 1;
            var dataAddress = reader.Address;
            reader.Skip(length);

            return new ZProperty(reader.Memory, propertyTable, address, number, dataAddress, length);
        }

        private static ZProperty NextProperty_V4(this IMemoryReader reader, byte sizeByte, ZPropertyTable propertyTable)
        {
            var address = reader.Address - 1;
            var number = sizeByte & 0x3f; // number is in the bottom 6 bites

            int length;
            if ((sizeByte & 0x80) == 0x80) // if bit 7 is set
            {
                var nextByte = reader.NextByte() & 0x3f;
                length = nextByte == 0 ? 64 : nextByte;
            }
            else if ((sizeByte & 0x40) == 0x40) // if bit 6 is set
            {
                length = 2;
            }
            else
            {
                length = 1;
            }

            var dataAddress = reader.Address;
            reader.Skip(length);

            return new ZProperty(reader.Memory, propertyTable, address, number, dataAddress, length);
        }

        public static ZProperty NextProperty(this IMemoryReader reader, int version, ZPropertyTable propertyTable)
        {
            var sizeByte = reader.NextByte();
            if (sizeByte == 0)
            {
                return null;
            }

            if (version >= 1 && version <= 3)
            {
                return reader.NextProperty_V1(sizeByte, propertyTable);
            }
            else if (version >= 4 && version <= 8)
            {
                return reader.NextProperty_V4(sizeByte, propertyTable);
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static void SkipShortName(this IMemoryReader reader)
        {
            var length = reader.NextByte();
            reader.Skip(length * 2);
        }

        public static void SkipProperties(this IMemoryReader reader, int version)
        {
            while (true)
            {
                var sizeByte = reader.NextByte();
                if (sizeByte == 0)
                {
                    return;
                }

                int dataLength;
                if (version >= 1 && version <= 3)
                {
                    dataLength = (sizeByte / 32) + 1;
                }
                else if ((sizeByte & 0x80) == 0x80)
                {
                    var nextByte = reader.NextByte() & 0x3f;
                    dataLength = nextByte == 0 ? 64 : nextByte;
                }
                else if ((sizeByte & 0x40) == 0x40)
                {
                    dataLength = 2;
                }
                else
                {
                    dataLength = 1;
                }

                reader.Skip(dataLength);
            }
        }
    }
}
