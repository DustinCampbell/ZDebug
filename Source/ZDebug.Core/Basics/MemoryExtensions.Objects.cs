using System;

namespace ZDebug.Core.Basics
{
    internal static partial class MemoryExtensions
    {
        private static int GetPropertyDefaultsCount(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 31;
            }
            else if (version >= 4 && version <= 8)
            {
                return 63;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetPropertyDefaultsTableSize(byte version)
        {
            return GetPropertyDefaultsCount(version) * 2;
        }

        private static int GetMaxObjects(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 255;
            }
            else if (version >= 4 && version <= 8)
            {
                return 65535;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetObjectEntrySize(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 9;
            }
            else if (version >= 4 && version <= 8)
            {
                return 14;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetObjectAttributeBytesSize(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 4;
            }
            else if (version >= 4 && version <= 8)
            {
                return 6;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetObjectParentOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 4;
            }
            else if (version >= 4 && version <= 8)
            {
                return 6;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetObjectSiblingOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 5;
            }
            else if (version >= 4 && version <= 8)
            {
                return 8;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetObjectChildOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 6;
            }
            else if (version >= 4 && version <= 8)
            {
                return 10;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetObjectPropertyTableAddressOffset(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 7;
            }
            else if (version >= 4 && version <= 8)
            {
                return 12;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static int GetObjectNumberSize(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 1;
            }
            else if (version >= 4 && version <= 8)
            {
                return 2;
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int GetObjectEntryAddress(this Memory memory, int objNum)
        {
            if (objNum < 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var version = memory.ReadVersion();
            var propertyDefaultsTableSize = GetPropertyDefaultsTableSize(version);
            var entrySize = GetObjectEntrySize(version);

            var objectTableAddress = memory.ReadObjectTableAddress();
            var offset = propertyDefaultsTableSize + ((objNum - 1) * entrySize);

            return objectTableAddress + offset;
        }

        public static ushort ReadPropertyDefault(this Memory memory, int propNum)
        {
            var version = memory.ReadVersion();
            var propertyDefaultsCount = GetPropertyDefaultsCount(version);
            if (propNum < 1 || propNum > propertyDefaultsCount)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var objectTableAddres = memory.ReadObjectTableAddress();
            var offset = (propNum - 1) * 2;

            return memory.ReadWord(objectTableAddres + offset);
        }

        public static byte[] ReadObjectAttributeBytes(this Memory memory, int objNum)
        {
            var version = memory.ReadVersion();
            var attributeBytesSize = GetObjectAttributeBytesSize(version);
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);

            return memory.ReadBytes(objectEntryAddress, attributeBytesSize);
        }

        public static void WriteObjectAttributeBytes(this Memory memory, int objNum, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            var version = memory.ReadVersion();
            var attributeBytesSize = GetObjectAttributeBytesSize(version);

            if (bytes.Length != attributeBytesSize)
            {
                throw new ArgumentException("Invalid attribute byte length: " + bytes.Length, "bytes");
            }

            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);

            memory.WriteBytes(objectEntryAddress, bytes);
        }

        private static int ReadObjectNumber(this Memory memory, byte version, int address)
        {
            var numberSize = GetObjectNumberSize(version);

            if (numberSize == 1)
            {
                return memory.ReadByte(address);
            }
            else if (numberSize == 2)
            {
                return memory.ReadWord(address);
            }
            else
            {
                throw new InvalidOperationException("Invalid object number size: " + numberSize);
            }
        }

        private static void WriteObjectNumber(this Memory memory, byte version, int address, int value)
        {
            var numberSize = GetObjectNumberSize(version);

            if (numberSize == 1)
            {
                if (value < byte.MinValue || value > byte.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                memory.WriteByte(address, (byte)value);
            }
            else if (numberSize == 2)
            {
                if (value < ushort.MinValue || value > ushort.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                memory.WriteWord(address, (ushort)value);
            }
            else
            {
                throw new InvalidOperationException("Invalid object number size: " + numberSize);
            }
        }

        public static int ReadObjectParentNumber(this Memory memory, int objNum)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var parentOffset = GetObjectParentOffset(version);

            return memory.ReadObjectNumber(version, objectEntryAddress + parentOffset);
        }

        public static void WriteObjectParentNumber(this Memory memory, int objNum, int parentObjNum)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var parentOffset = GetObjectParentOffset(version);

            memory.WriteObjectNumber(version, objectEntryAddress + parentOffset, parentObjNum);
        }

        public static int ReadObjectSiblingNumber(this Memory memory, int objNum)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var siblingOffset = GetObjectSiblingOffset(version);

            return memory.ReadObjectNumber(version, objectEntryAddress + siblingOffset);
        }

        public static void WriteObjectSiblingNumber(this Memory memory, int objNum, int siblingObjNum)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var siblingOffset = GetObjectSiblingOffset(version);

            memory.WriteObjectNumber(version, objectEntryAddress + siblingOffset, siblingObjNum);
        }

        public static int ReadObjectChildNumber(this Memory memory, int objNum)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var childOffset = GetObjectChildOffset(version);

            return memory.ReadObjectNumber(version, objectEntryAddress + childOffset);
        }

        public static void WriteObjectChildNumber(this Memory memory, int objNum, int childObjNum)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var childOffset = GetObjectChildOffset(version);

            memory.WriteObjectNumber(version, objectEntryAddress + childOffset, childObjNum);
        }

        public static ushort ReadObjectPropertyTableAddress(this Memory memory, int objNum)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var propertyTableAddressOffset = GetObjectPropertyTableAddressOffset(version);

            return memory.ReadWord(objectEntryAddress + propertyTableAddressOffset);
        }

        public static void WriteObjectPropertyTableAddress(this Memory memory, int objNum, ushort address)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objNum);
            var propertyTableAddressOffset = GetObjectPropertyTableAddressOffset(version);

            memory.WriteWord(objectEntryAddress + propertyTableAddressOffset, address);
        }

        /// <summary>
        /// Because this operation walks the entire object table it can be expensive.
        /// </summary>
        public static int GetObjectCount(this Memory memory)
        {
            var version = memory.ReadVersion();
            var maxObjects = GetMaxObjects(version);
            var objectTableAddress = memory.ReadObjectTableAddress();
            var objectEntrySize = GetObjectEntrySize(version);
            var propertyTableAddressOffset = GetObjectPropertyTableAddressOffset(version);
            var propertyDefaultsTableSize = GetPropertyDefaultsTableSize(version);

            var address = objectTableAddress + propertyDefaultsTableSize;
            var smallestPropertyTableAddress = Int32.MaxValue;

            for (int i = 1; i <= maxObjects; i++)
            {
                if (address >= smallestPropertyTableAddress)
                {
                    return i - 1;
                }

                var propertyTableAddress = memory.ReadWord(address + propertyTableAddressOffset);
                smallestPropertyTableAddress = Math.Min(smallestPropertyTableAddress, propertyTableAddress);

                address += objectEntrySize;
            }

            throw new InvalidOperationException("Could not find the end of the object table");
        }

        public static ushort[] ReadShortName(this Memory memory, int address)
        {
            var length = memory.ReadByte(address);
            return memory.ReadWords(address + 1, length);
        }

        public static ushort[] ReadObjectShortName(this Memory memory, int objNum)
        {
            var propertyTableAddress = memory.ReadObjectPropertyTableAddress(objNum);
            return memory.ReadShortName(propertyTableAddress);
        }
    }
}
