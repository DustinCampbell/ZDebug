using System.Collections;
using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Objects
{
    public class ZPropertyTable : IIndexedEnumerable<ZProperty>
    {
        private readonly Memory memory;
        private readonly int address;

        private readonly ZProperty[] properties;

        internal ZPropertyTable(Memory memory, int address)
        {
            this.memory = memory;
            this.address = address;

            properties = memory.ReadPropertyTableProperties(this);
        }

        public ushort[] GetShortNameZWords()
        {
            return memory.ReadShortName(address);
        }

        public bool Contains(int propNum)
        {
            for (int i = 0; i < properties.Length; i++)
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
            for (int i = 0; i < properties.Length; i++)
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
            get { return properties.Length; }
        }

        public IEnumerator<ZProperty> GetEnumerator()
        {
            for (int i = 0; i < properties.Length; i++)
            {
                yield return properties[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
