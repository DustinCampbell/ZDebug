using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Objects
{
    public class ZObjectTable : IIndexedEnumerable<ZObject>
    {
        private readonly Memory memory;
        private readonly int address;

        private readonly Dictionary<int, ZPropertyTable> propertyTables;
        private readonly ReadOnlyCollection<ZObject> objects;

        internal ZObjectTable(Memory memory)
        {
            this.memory = memory;
            this.address = memory.ReadObjectTableAddress();
            this.propertyTables = new Dictionary<int, ZPropertyTable>();

            this.objects = new ReadOnlyCollection<ZObject>(
                memory.ReadObjectTableObjects(this));
        }

        internal ZPropertyTable GetPropertyTable(int address)
        {
            ZPropertyTable propertyTable;
            if (!propertyTables.TryGetValue(address, out propertyTable))
            {
                propertyTable = new ZPropertyTable(memory, address);
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
            return memory.ReadPropertyDefault(propNum);
        }

        public ZObject this[int index]
        {
            get { return objects[index]; }
        }

        public int Count
        {
            get { return objects.Count; }
        }

        public IEnumerator<ZObject> GetEnumerator()
        {
            foreach (var obj in objects)
            {
                yield return obj;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
