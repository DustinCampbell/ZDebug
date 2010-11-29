using System;
using System.Collections.Generic;
using ZDebug.Core.Objects;

namespace ZDebug.Core.Basics
{
    internal static partial class MemoryExtensions
    {
        public static int GetObjectEntryAddress(this Memory memory, int objNum)
        {
            if (objNum < 1)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var version = memory.ReadVersion();
            var propertyDefaultsTableSize = ObjectHelpers.GetPropertyDefaultsTableSize(version);
            var entrySize = ObjectHelpers.GetEntrySize(version);

            var objectTableAddress = memory.ReadObjectTableAddress();
            var offset = propertyDefaultsTableSize + ((objNum - 1) * entrySize);

            return objectTableAddress + offset;
        }

        public static ushort ReadPropertyDefault(this Memory memory, int propNum)
        {
            var version = memory.ReadVersion();
            var propertyDefaultsCount = ObjectHelpers.GetPropertyDefaultsCount(version);
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
            var attributeBytesSize = ObjectHelpers.GetAttributeBytesSize(version);

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
            var attributeBytesSize = ObjectHelpers.GetAttributeBytesSize(version);

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
            var attributeCount = ObjectHelpers.GetAttributeCount(version);

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

        public static bool[] GetAllAttributeByObjectAddress(this Memory memory, int objAddress)
        {
            var version = memory.ReadVersion();
            var attributeCount = ObjectHelpers.GetAttributeCount(version);

            var attributeBytes = memory.ReadAttributeBytesByObjectAddress(objAddress);

            var result = new bool[attributeCount];

            for (int i = 0; i < attributeCount; i++)
            {
                var byteIdx = i / 8;
                var bitMask = 1 << (7 - (i % 8));

                result[i] = (attributeBytes[byteIdx] & bitMask) == bitMask;
            }

            return result;
        }

        public static bool[] GetAllAttributeByObjectNumber(this Memory memory, int objNum)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            return memory.GetAllAttributeByObjectAddress(objAddress);
        }

        public static void SetAttributeValueByObjectAddress(this Memory memory, int objAddress, int attribute, bool value)
        {
            var version = memory.ReadVersion();
            var attributeCount = ObjectHelpers.GetAttributeCount(version);

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

        public static void SetAttributeValueByObjectNumber(this Memory memory, int objNum, int attribute, bool value)
        {
            var objAddress = GetObjectEntryAddress(memory, objNum);

            memory.SetAttributeValueByObjectAddress(objAddress, attribute, value);
        }

        private static int ReadObjectNumber(this Memory memory, byte version, int address)
        {
            var numberSize = ObjectHelpers.GetNumberSize(version);

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
            var numberSize = ObjectHelpers.GetNumberSize(version);

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
            var parentOffset = ObjectHelpers.GetParentOffset(version);

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
            var parentOffset = ObjectHelpers.GetParentOffset(version);

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
            var siblingOffset = ObjectHelpers.GetSiblingOffset(version);

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
            var siblingOffset = ObjectHelpers.GetSiblingOffset(version);

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
            var childOffset = ObjectHelpers.GetChildOffset(version);

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
            var childOffset = ObjectHelpers.GetChildOffset(version);

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
            var propertyTableAddressOffset = ObjectHelpers.GetPropertyTableAddressOffset(version);

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
            var propertyTableAddressOffset = ObjectHelpers.GetPropertyTableAddressOffset(version);

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
            var maxObjects = ObjectHelpers.GetMaxObjects(version);
            var objectEntrySize = ObjectHelpers.GetEntrySize(version);
            var propertyTableAddressOffset = ObjectHelpers.GetPropertyTableAddressOffset(version);
            var propertyDefaultsTableSize = ObjectHelpers.GetPropertyDefaultsTableSize(version);

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

            if (address >= smallestPropertyTableAddress)
            {
                return objects;
            }

            throw new InvalidOperationException("Could not find the end of the object table");
        }

        /// <summary>
        /// Because this operation walks the entire object table it can be expensive.
        /// </summary>
        public static int GetObjectCount(this Memory memory)
        {
            var version = memory.ReadVersion();
            var maxObjects = ObjectHelpers.GetMaxObjects(version);
            var objectTableAddress = memory.ReadObjectTableAddress();
            var objectEntrySize = ObjectHelpers.GetEntrySize(version);
            var propertyTableAddressOffset = ObjectHelpers.GetPropertyTableAddressOffset(version);
            var propertyDefaultsTableSize = ObjectHelpers.GetPropertyDefaultsTableSize(version);

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

            if (address >= smallestPropertyTableAddress)
            {
                return maxObjects;
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
