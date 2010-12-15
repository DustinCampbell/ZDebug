using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Objects
{
    public class ZPropertyTable : IIndexedEnumerable<ZProperty>
    {
        private readonly Memory memory;
        private readonly int address;

        private readonly ReadOnlyCollection<ZProperty> properties;

        internal ZPropertyTable(Memory memory, int address)
        {
            this.memory = memory;
            this.address = address;

            properties = new ReadOnlyCollection<ZProperty>(
                memory.ReadPropertyTableProperties(this));
        }

        public ushort[] GetShortNameZWords()
        {
            return memory.ReadShortName(address);
        }

        public bool Contains(int propNum)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i].Number == propNum)
                {
                    return true;
                }
            }

            return false;
        }

        public ZProperty GetByNumber(int propNum)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                var p = properties[i];
                if (p.Number == propNum)
                {
                    return p;
                }
            }

            return null;
        }

        public int Address
        {
            get { return address; }
        }

        public ZProperty this[int index]
        {
            get { return properties[index]; }
        }

        public int Count
        {
            get { return properties.Count; }
        }

        public IEnumerator<ZProperty> GetEnumerator()
        {
            foreach (var prop in properties)
            {
                yield return prop;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
