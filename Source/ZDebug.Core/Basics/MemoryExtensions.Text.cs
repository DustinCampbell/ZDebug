using System;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    internal static partial class MemoryExtensions
    {
        public static char[][] ReadCustomAlphabetTable(this Memory memory, int address)
        {
            var result = new char[3][];

            Converter<byte, char> byteToChar = b =>
            {
                var ch = (char)b;
                if (ch == '^')
                {
                    return '\n';
                }
                else
                {
                    return ch;
                }
            };

            var reader = memory.CreateReader(address);
            result[0] = "??????".ToCharArray().Concat(reader.NextBytes(26).ConvertAll(byteToChar));
            result[1] = "??????".ToCharArray().Concat(reader.NextBytes(26).ConvertAll(byteToChar));

            reader.Skip(1);
            result[2] = "???????".ToCharArray().Concat(reader.NextBytes(25).ConvertAll(byteToChar));

            return result;
        }

        public static ushort[] ReadAbbreviation(this Memory memory, int index)
        {
            if (index < 0 || index > 95)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var abbreviationsTableAddress = Header.ReadAbbreviationsTableAddress(memory.Bytes);
            var abbreviationAddress = (2 * memory.ReadWord(abbreviationsTableAddress + (index * 2)));

            return memory.ReadZWords(abbreviationAddress);
        }

        /// <summary>
        /// Reads the Z-words at the specified <paramref name="address"/> from the given <see cref="Memory"/>.
        /// </summary>
        public static ushort[] ReadZWords(this Memory memory, int address)
        {
            if (memory == null)
            {
                throw new ArgumentNullException("memory");
            }

            int count = 0;
            while (true)
            {
                var zword = memory.ReadWord(address + (count++ * 2));
                if ((zword & 0x8000) != 0)
                {
                    break;
                }
            }

            return memory.ReadWords(address, count);
        }
    }
}
