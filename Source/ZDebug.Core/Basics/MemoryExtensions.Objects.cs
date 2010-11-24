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

        private static int GetObjectIndexSize(byte version)
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

        public static int GetObjectEntryAddress(this Memory memory, int objIndex)
        {
            if (objIndex < 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var version = memory.ReadVersion();
            var propertyDefaultsTableSize = GetPropertyDefaultsTableSize(version);
            var entrySize = GetObjectEntrySize(version);

            var objectTableAddress = memory.ReadObjectTableAddress();
            var offset = propertyDefaultsTableSize + ((objIndex - 1) * entrySize);

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

        public static byte[] ReadObjectAttributeBytes(this Memory memory, int objIndex)
        {
            var version = memory.ReadVersion();
            var attributeBytesSize = GetObjectAttributeBytesSize(version);
            var objectEntryAddress = GetObjectEntryAddress(memory, objIndex);

            return memory.ReadBytes(objectEntryAddress, attributeBytesSize);
        }

        private static int ReadObjectIndex(this Memory memory, int address, byte version)
        {
            var indexSize = GetObjectIndexSize(version);

            if (indexSize == 1)
            {
                return memory.ReadByte(address);
            }
            else if (indexSize == 2)
            {
                return memory.ReadWord(address);
            }
            else
            {
                throw new InvalidOperationException("Invalid object index size: " + indexSize);
            }
        }

        public static int ReadObjectParentIndex(this Memory memory, int objIndex)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objIndex);
            var parentOffset = GetObjectParentOffset(version);

            return memory.ReadObjectIndex(objectEntryAddress + parentOffset, version);
        }

        public static int ReadObjectSiblingIndex(this Memory memory, int objIndex)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objIndex);
            var siblingOffset = GetObjectSiblingOffset(version);

            return memory.ReadObjectIndex(objectEntryAddress + siblingOffset, version);
        }

        public static int ReadObjectChildIndex(this Memory memory, int objIndex)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objIndex);
            var childOffset = GetObjectChildOffset(version);

            return memory.ReadObjectIndex(objectEntryAddress + childOffset, version);
        }

        public static ushort ReadObjectPropertyTableAddress(this Memory memory, int objIndex)
        {
            var version = memory.ReadVersion();
            var objectEntryAddress = GetObjectEntryAddress(memory, objIndex);
            var propertyTableAddressOffset = GetObjectPropertyTableAddressOffset(version);

            return memory.ReadWord(objectEntryAddress + propertyTableAddressOffset);
        }
    }
}
