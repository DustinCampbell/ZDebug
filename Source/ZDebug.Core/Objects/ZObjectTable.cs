using System;
using System.Collections;
using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Objects
{
    public class ZObjectTable : IIndexedEnumerable<ZObject>
    {
        private readonly Memory memory;
        private readonly byte[] bytes;
        private readonly ZText ztext;
        private readonly byte version;
        private readonly ushort address;

        private readonly ushort maxObjects;
        private readonly byte maxProperties;
        private readonly byte propertyDefaultsTableSize;
        private readonly ushort objectEntriesAddress;
        private readonly byte entrySize;
        private readonly byte attributeBytesSize;
        private readonly byte attributeCount;
        private readonly byte numberSize;
        private readonly byte parentOffset;
        private readonly byte siblingOffset;
        private readonly byte childOffset;
        private readonly byte propertyTableAddressOffset;

        private readonly IntegerMap<ZPropertyTable> propertyTables;
        private readonly ZObject[] objects;

        internal ZObjectTable(Memory memory, ZText ztext)
        {
            this.memory = memory;
            this.bytes = memory.Bytes;
            this.ztext = ztext;
            this.version = memory.ReadVersion();
            this.address = memory.ReadObjectTableAddress();

            this.maxObjects = (ushort)(version <= 3 ? 255 : 65535);
            this.maxProperties = (byte)(version <= 3 ? 31 : 63);
            this.propertyDefaultsTableSize = (byte)(maxProperties * 2);
            this.objectEntriesAddress = (ushort)(address + propertyDefaultsTableSize);
            this.entrySize = (byte)(version <= 3 ? 9 : 14);
            this.attributeBytesSize = (byte)(version <= 3 ? 4 : 6);
            this.attributeCount = (byte)(version <= 3 ? 32 : 48);
            this.numberSize = (byte)(version <= 3 ? 1 : 2);
            this.parentOffset = (byte)(version <= 3 ? 4 : 6);
            this.siblingOffset = (byte)(version <= 3 ? 5 : 8);
            this.childOffset = (byte)(version <= 3 ? 6 : 10);
            this.propertyTableAddressOffset = (byte)(version <= 3 ? 7 : 12);

            this.objects = ReadAllObjects();

            this.propertyTables = new IntegerMap<ZPropertyTable>(objects.Length);
        }

        public byte MaxProperties
        {
            get { return maxProperties; }
        }

        internal ushort GetObjectEntryAddress(ushort objNum)
        {
            if (objNum < 1)
            {
                throw new ArgumentOutOfRangeException("objNum");
            }

            return (ushort)(objectEntriesAddress + ((objNum - 1) * entrySize));
        }

        internal ushort ReadPropertyDefault(int propNum)
        {
            if (propNum < 1 || propNum > maxProperties)
            {
                throw new ArgumentOutOfRangeException("propNum");
            }

            return bytes.ReadWord(address + ((propNum - 1) * 2));
        }

        internal byte[] ReadAttributeBytesByObjectAddress(ushort objAddress)
        {
            return memory.ReadBytes(objAddress, attributeBytesSize);
        }

        internal byte[] ReadAttributeBytesByObjectNumber(ushort objNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            return ReadAttributeBytesByObjectAddress(objAddress);
        }

        internal void WriteAttributeBytesByObjectAddress(ushort objAddress, byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            if (bytes.Length != attributeBytesSize)
            {
                throw new ArgumentException("Invalid attribute byte length: " + bytes.Length, "bytes");
            }

            memory.WriteBytes(objAddress, bytes);
        }

        internal void WriteAttributeBytesByObjectNumber(ushort objNum, byte[] bytes)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            WriteAttributeBytesByObjectAddress(objAddress, bytes);
        }

        internal bool HasAttributeByObjectAddress(ushort objAddress, byte attribute)
        {
            if (attribute < 0 || attribute >= attributeCount)
            {
                throw new ArgumentOutOfRangeException("attribute");
            }

            int byteIdx = attribute / 8;
            int bitMask = 1 << (7 - (attribute % 8));

            byte b = bytes[objAddress + byteIdx];

            return (b & bitMask) != 0;
        }

        internal bool HasAttributeByObjectNumber(ushort objNum, byte attribute)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            return HasAttributeByObjectAddress(objAddress, attribute);
        }

        internal bool[] GetAllAttributeByObjectAddress(ushort objAddress)
        {
            byte[] attributeBytes = ReadAttributeBytesByObjectAddress(objAddress);

            bool[] result = new bool[attributeCount];

            for (int i = 0; i < attributeCount; i++)
            {
                var byteIdx = i / 8;
                var bitMask = 1 << (7 - (i % 8));

                result[i] = (attributeBytes[byteIdx] & bitMask) == bitMask;
            }

            return result;
        }

        internal bool[] GetAllAttributeByObjectNumber(ushort objNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            return GetAllAttributeByObjectAddress(objAddress);
        }

        internal void SetAttributeValueByObjectAddress(ushort objAddress, byte attribute, bool value)
        {
            if (attribute < 0 || attribute >= attributeCount)
            {
                throw new ArgumentOutOfRangeException("attribute");
            }

            byte[] attributeBytes = ReadAttributeBytesByObjectAddress(objAddress);

            int byteIdx = attribute / 8;
            int bitMask = 1 << (7 - (attribute % 8));

            attributeBytes[byteIdx] = value
                ? (byte)(attributeBytes[byteIdx] | bitMask)
                : (byte)(attributeBytes[byteIdx] & ~bitMask);

            WriteAttributeBytesByObjectAddress(objAddress, attributeBytes);
        }

        internal void SetAttributeValueByObjectNumber(ushort objNum, byte attribute, bool value)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            SetAttributeValueByObjectAddress(objAddress, attribute, value);
        }

        internal ushort ReadObjectNumber(ushort address)
        {
            if (numberSize == 1)
            {
                return bytes[address];
            }
            else if (numberSize == 2)
            {
                return bytes.ReadWord(address);
            }
            else
            {
                throw new InvalidOperationException("Invalid object number size: " + numberSize);
            }
        }

        internal void WriteObjectNumber(ushort address, ushort value)
        {
            if (numberSize == 1)
            {
                bytes[address] = (byte)value;
            }
            else if (numberSize == 2)
            {
                bytes.WriteWord(address, value);
            }
            else
            {
                throw new InvalidOperationException("Invalid object number size: " + numberSize);
            }
        }

        internal ushort ReadParentNumberByObjectAddress(ushort objAddress)
        {
            return ReadObjectNumber((ushort)(objAddress + parentOffset));
        }

        internal ushort ReadParentNumberByObjectNumber(ushort objNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            return ReadParentNumberByObjectAddress(objAddress);
        }

        internal void WriteParentNumberByObjectAddress(ushort objAddress, ushort parentObjNum)
        {
            WriteObjectNumber((ushort)(objAddress + parentOffset), parentObjNum);
        }

        internal void WriteParentNumberByObjectNumber(ushort objNum, ushort parentObjNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            WriteParentNumberByObjectAddress(objAddress, parentObjNum);
        }

        internal ushort ReadSiblingNumberByObjectAddress(ushort objAddress)
        {
            return ReadObjectNumber((ushort)(objAddress + siblingOffset));
        }

        internal ushort ReadSiblingNumberByObjectNumber(ushort objNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            return ReadSiblingNumberByObjectAddress(objAddress);
        }

        internal void WriteSiblingNumberByObjectAddress(ushort objAddress, ushort siblingObjNum)
        {
            WriteObjectNumber((ushort)(objAddress + siblingOffset), siblingObjNum);
        }

        internal void WriteSiblingNumberByObjectNumber(ushort objNum, ushort siblingObjNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            WriteSiblingNumberByObjectAddress(objAddress, siblingObjNum);
        }

        internal ushort ReadChildNumberByObjectAddress(ushort objAddress)
        {
            return ReadObjectNumber((ushort)(objAddress + childOffset));
        }

        internal ushort ReadChildNumberByObjectNumber(ushort objNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            return ReadChildNumberByObjectAddress(objAddress);
        }

        internal void WriteChildNumberByObjectAddress(ushort objAddress, ushort childObjNum)
        {
            WriteObjectNumber((ushort)(objAddress + childOffset), childObjNum);
        }

        internal void WriteChildNumberByObjectNumber(ushort objNum, ushort childObjNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            WriteChildNumberByObjectAddress(objAddress, childObjNum);
        }

        internal ushort ReadPropertyTableAddressByObjectAddress(ushort objAddress)
        {
            return bytes.ReadWord(objAddress + propertyTableAddressOffset);
        }

        internal ushort ReadPropertyTableAddressByObjectNumber(ushort objNum)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            return ReadPropertyTableAddressByObjectAddress(objAddress);
        }

        internal void WritePropertyTableAddressByObjectAddress(ushort objAddress, ushort value)
        {
            bytes.WriteWord(objAddress + propertyTableAddressOffset, value);
        }

        internal void WritePropertyTableAddressByObjectNumber(ushort objNum, ushort value)
        {
            ushort objAddress = GetObjectEntryAddress(objNum);

            WritePropertyTableAddressByObjectAddress(objAddress, value);
        }

        internal ZObject[] ReadAllObjects()
        {
            ushort address = objectEntriesAddress;
            ushort smallestPropertyTableAddress = UInt16.MaxValue;

            var objects = new List<ZObject>();

            for (ushort i = 1; i <= maxObjects; i++)
            {
                if (address >= smallestPropertyTableAddress)
                {
                    return objects.ToArray();
                }

                objects.Add(new ZObject(memory, this, ztext, address, i));

                var propertyTableAddress = bytes.ReadWord(address + propertyTableAddressOffset);
                smallestPropertyTableAddress = Math.Min(smallestPropertyTableAddress, propertyTableAddress);

                address += entrySize;
            }

            if (address >= smallestPropertyTableAddress)
            {
                return objects.ToArray();
            }

            throw new InvalidOperationException("Could not find the end of the object table");
        }

        /// <summary>
        /// Because this operation walks the entire object table it can be expensive.
        /// </summary>
        internal int GetObjectCount()
        {
            ushort address = objectEntriesAddress;
            ushort smallestPropertyTableAddress = UInt16.MaxValue;

            for (int i = 1; i <= maxObjects; i++)
            {
                if (address >= smallestPropertyTableAddress)
                {
                    return i - 1;
                }

                var propertyTableAddress = memory.ReadWord(address + propertyTableAddressOffset);
                smallestPropertyTableAddress = Math.Min(smallestPropertyTableAddress, propertyTableAddress);

                address += entrySize;
            }

            if (address >= smallestPropertyTableAddress)
            {
                return maxObjects;
            }

            throw new InvalidOperationException("Could not find the end of the object table");
        }

        internal ushort[] ReadShortName(ushort address)
        {
            var length = bytes[address];
            return memory.ReadWords(address + 1, length);
        }

        internal ushort[] ReadObjectShortName(ushort objNum)
        {
            var propertyTableAddress = ReadPropertyTableAddressByObjectNumber(objNum);
            return ReadShortName(propertyTableAddress);
        }

        internal byte ReadPropertyDataLength(ushort dataAddress)
        {
            if (dataAddress == 0)
            {
                return 0;
            }

            byte sizeByte = bytes[dataAddress - 1];

            byte dataLength;
            if (version <= 3)
            {
                dataLength = (byte)((sizeByte >> 5) + 1);
            }
            else if ((sizeByte & 0x80) == 0)
            {
                dataLength = (byte)((sizeByte >> 6) + 1);
            }
            else
            {
                dataLength = (byte)(sizeByte & 0x3F);
            }

            if (dataLength == 0)
            {
                dataLength = 64;
            }

            return dataLength;
        }

        internal ZProperty[] ReadPropertyTableProperties(ZPropertyTable propertyTable)
        {
            // read properties...
            var props = new List<ZProperty>();
            var reader = memory.CreateReader(propertyTable.Address);

            reader.SkipShortName();

            var version = memory.ReadVersion();
            var index = 0;
            var prop = reader.NextProperty(version, propertyTable, index);
            while (prop != null)
            {
                props.Add(prop);
                prop = reader.NextProperty(version, propertyTable, ++index);
            }

            return props.ToArray();
        }

        internal ushort? TryReadLeftSiblingNumberByObjectNumber(ushort objNum)
        {
            ushort parentNum = ReadParentNumberByObjectNumber(objNum);
            if (parentNum == 0)
            {
                return null;
            }

            ushort parentChildNum = ReadChildNumberByObjectNumber(parentNum);
            if (parentChildNum == objNum)
            {
                return null;
            }

            ushort next = parentChildNum;
            while (next != 0)
            {
                ushort siblingNum = ReadSiblingNumberByObjectNumber(next);
                if (siblingNum == objNum)
                {
                    return next;
                }
                else
                {
                    next = siblingNum;
                }
            }

            return null;
        }

        internal void RemoveObjectFromParentByNumber(ushort objNum)
        {
            ushort? leftSiblingNum = TryReadLeftSiblingNumberByObjectNumber(objNum);
            ushort rightSiblingNum = ReadSiblingNumberByObjectNumber(objNum);
            if (leftSiblingNum.HasValue)
            {
                WriteSiblingNumberByObjectNumber(leftSiblingNum.Value, rightSiblingNum);
            }

            ushort parentNum = ReadParentNumberByObjectNumber(objNum);
            if (parentNum != 0)
            {
                var parentChildNum = ReadChildNumberByObjectNumber(parentNum);
                if (parentChildNum == objNum)
                {
                    WriteChildNumberByObjectNumber(parentNum, rightSiblingNum);
                }
            }

            WriteParentNumberByObjectNumber(objNum, 0);
            WriteSiblingNumberByObjectNumber(objNum, 0);
        }

        internal void MoveObjectToDestinationByNumber(ushort objNum, ushort destNum)
        {
            RemoveObjectFromParentByNumber(objNum);

            if (destNum != 0)
            {
                WriteParentNumberByObjectNumber(objNum, destNum);
                ushort parentChildNum = ReadChildNumberByObjectNumber(destNum);
                WriteSiblingNumberByObjectNumber(objNum, parentChildNum);
                WriteChildNumberByObjectNumber(destNum, objNum);
            }
        }

        internal ZPropertyTable GetPropertyTable(ushort address)
        {
            ZPropertyTable propertyTable;
            if (!propertyTables.TryGetValue(address, out propertyTable))
            {
                propertyTable = new ZPropertyTable(this, address);
                propertyTables.Add(address, propertyTable);
            }

            return propertyTable;
        }

        public int Address
        {
            get { return address; }
        }

        public ZObject GetByNumber(int objNum)
        {
            return objects[objNum - 1];
        }

        public ushort GetPropertyDefault(int propNum)
        {
            return ReadPropertyDefault(propNum);
        }

        public ZObject this[int index]
        {
            get { return objects[index]; }
        }

        public int Count
        {
            get { return objects.Length; }
        }

        public IEnumerator<ZObject> GetEnumerator()
        {
            for (int i = 0; i < objects.Length; i++)
            {
                yield return objects[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
