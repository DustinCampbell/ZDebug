using System;
using System.Collections.Generic;
using ZDebug.Core.Objects;

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

        private static int GetObjectAttributeCount(byte version)
        {
            if (version >= 1 && version <= 3)
            {
                return 32;
            }
            else if (version >= 4 && version <= 8)
            {
                return 48;
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

        public static byte[] ReadAttributeBytesByObjectAddress(this Memory memory, int objAddress)
        {
            var version = memory.ReadVersion();
            var attributeBytesSize = GetObjectAttributeBytesSize(version);

            return memory.ReadBytes(objAddress, attributeBytesSize);
        }

        public static byte[] ReadAttributeBytesByObjectNumber(this Memory memory, int objNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            return memory.ReadAttributeBytesByObjectAddress(objAddress);
        }

        public static void WriteAttributeBytesByObjectAddress(this Memory memory, int objAddress, byte[] bytes)
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

            memory.WriteBytes(objAddress, bytes);
        }

        public static void WriteAttributeBytesByObjectNumber(this Memory memory, int objNum, byte[] bytes)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            memory.WriteAttributeBytesByObjectAddress(objAddress, bytes);
        }

        public static bool HasAttributeByObjectAddress(this Memory memory, int objAddress, int attribute)
        {
            var version = memory.ReadVersion();
            var attributeCount = GetObjectAttributeCount(version);

            if (attribute < 0 || attribute >= attributeCount)
            {
                throw new ArgumentOutOfRangeException("attribute");
            }

            var attributeBytes = memory.ReadAttributeBytesByObjectAddress(objAddress);

            var byteIdx = attribute / 8;
            var bitMask = 1 << (7 - (attribute % 8));

            return (attributeBytes[byteIdx] & bitMask) == bitMask;
        }

        public static bool HasAttributeByObjectNumber(this Memory memory, int objNum, int attribute)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            return memory.HasAttributeByObjectAddress(objAddress, attribute);
        }

        public static void SetAttributeByObjectAddress(this Memory memory, int objAddress, int attribute, bool value)
        {
            var version = memory.ReadVersion();
            var attributeCount = GetObjectAttributeCount(version);

            if (attribute < 0 || attribute >= attributeCount)
            {
                throw new ArgumentOutOfRangeException("attribute");
            }

            var attributeBytes = memory.ReadAttributeBytesByObjectAddress(objAddress);

            var byteIdx = attribute / 8;
            var bitMask = 1 << (7 - (attribute % 8));

            attributeBytes[byteIdx] = (byte)(attributeBytes[byteIdx] | bitMask);

            memory.WriteAttributeBytesByObjectAddress(objAddress, attributeBytes);
        }

        public static void SetAttributeByObjectNumber(this Memory memory, int objNum, int attribute, bool value)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            memory.SetAttributeByObjectAddress(objAddress, attribute, value);
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

        public static int ReadParentNumberByObjectAddress(this Memory memory, int objAddress)
        {
            var version = memory.ReadVersion();
            var parentOffset = GetObjectParentOffset(version);

            return memory.ReadObjectNumber(version, objAddress + parentOffset);
        }

        public static int ReadParentNumberByObjectNumber(this Memory memory, int objNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            return memory.ReadParentNumberByObjectAddress(objAddress);
        }

        public static void WriteParentNumberByObjectAddress(this Memory memory, int objAddress, int parentObjNum)
        {
            var version = memory.ReadVersion();
            var parentOffset = GetObjectParentOffset(version);

            memory.WriteObjectNumber(version, objAddress + parentOffset, parentObjNum);
        }

        public static void WriteParentNumberByObjectNumber(this Memory memory, int objNum, int parentObjNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            memory.WriteParentNumberByObjectAddress(objAddress, parentObjNum);
        }

        public static int ReadSiblingNumberByObjectAddress(this Memory memory, int objAddress)
        {
            var version = memory.ReadVersion();
            var siblingOffset = GetObjectSiblingOffset(version);

            return memory.ReadObjectNumber(version, objAddress + siblingOffset);
        }

        public static int ReadSiblingNumberByObjectNumber(this Memory memory, int objNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            return memory.ReadSiblingNumberByObjectAddress(objAddress);
        }

        public static void WriteSiblingNumberByObjectAddress(this Memory memory, int objAddress, int siblingObjNum)
        {
            var version = memory.ReadVersion();
            var siblingOffset = GetObjectSiblingOffset(version);

            memory.WriteObjectNumber(version, objAddress + siblingOffset, siblingObjNum);
        }

        public static void WriteSiblingNumberByObjectNumber(this Memory memory, int objNum, int siblingObjNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            memory.WriteSiblingNumberByObjectAddress(objAddress, siblingObjNum);
        }

        public static int ReadChildNumberByObjectAddress(this Memory memory, int objAddress)
        {
            var version = memory.ReadVersion();
            var childOffset = GetObjectChildOffset(version);

            return memory.ReadObjectNumber(version, objAddress + childOffset);
        }

        public static int ReadChildNumberByObjectNumber(this Memory memory, int objNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            return memory.ReadChildNumberByObjectAddress(objAddress);
        }

        public static void WriteChildNumberByObjectAddress(this Memory memory, int objAddress, int childObjNum)
        {
            var version = memory.ReadVersion();
            var childOffset = GetObjectChildOffset(version);

            memory.WriteObjectNumber(version, objAddress + childOffset, childObjNum);
        }

        public static void WriteChildNumberByObjectNumber(this Memory memory, int objNum, int childObjNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            memory.WriteChildNumberByObjectAddress(objAddress, childObjNum);
        }

        public static ushort ReadPropertyTableAddressByObjectAddress(this Memory memory, int objAddress)
        {
            var version = memory.ReadVersion();
            var propertyTableAddressOffset = GetObjectPropertyTableAddressOffset(version);

            return memory.ReadWord(objAddress + propertyTableAddressOffset);
        }

        public static ushort ReadPropertyTableAddressByObjectNumber(this Memory memory, int objNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            return memory.ReadPropertyTableAddressByObjectAddress(objAddress);
        }

        public static void WritePropertyTableAddressByObjectAddress(this Memory memory, int objAddress, ushort value)
        {
            var version = memory.ReadVersion();
            var propertyTableAddressOffset = GetObjectPropertyTableAddressOffset(version);

            memory.WriteWord(objAddress + propertyTableAddressOffset, value);
        }

        public static void WritePropertyTableAddressByObjectNumber(this Memory memory, int objNum, ushort value)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            memory.WritePropertyTableAddressByObjectNumber(objAddress, value);
        }

        public static IList<ZObject> ReadObjectTableObjects(this Memory memory, ZObjectTable objectTable)
        {
            var version = memory.ReadVersion();
            var maxObjects = GetMaxObjects(version);
            var objectEntrySize = GetObjectEntrySize(version);
            var propertyTableAddressOffset = GetObjectPropertyTableAddressOffset(version);
            var propertyDefaultsTableSize = GetPropertyDefaultsTableSize(version);

            var address = objectTable.Address + propertyDefaultsTableSize;
            var smallestPropertyTableAddress = Int32.MaxValue;

            var objects = new List<ZObject>();

            for (int i = 1; i <= maxObjects; i++)
            {
                if (address >= smallestPropertyTableAddress)
                {
                    return objects;
                }

                objects.Add(new ZObject(memory, objectTable, address, i));

                var propertyTableAddress = memory.ReadWord(address + propertyTableAddressOffset);
                smallestPropertyTableAddress = Math.Min(smallestPropertyTableAddress, propertyTableAddress);

                address += objectEntrySize;
            }

            throw new InvalidOperationException("Could not find the end of the object table");
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
            var propertyTableAddress = memory.ReadPropertyTableAddressByObjectNumber(objNum);
            return memory.ReadShortName(propertyTableAddress);
        }

        public static IList<ZProperty> ReadPropertyTableProperties(this Memory memory, ZPropertyTable propertyTable)
        {
            // read properties...
            var props = new List<ZProperty>();
            var reader = memory.CreateReader(propertyTable.Address);

            reader.SkipShortName();

            var version = memory.ReadVersion();
            ZProperty prop = reader.NextProperty(version, propertyTable);
            while (prop != null)
            {
                props.Add(prop);
                prop = reader.NextProperty(version, propertyTable);
            }

            return props;
        }
    }
}
