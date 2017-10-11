using System.Collections;
using System.Collections.Generic;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Objects
{
    public class ZPropertyTable : IIndexedEnumerable<ZProperty>
    {
        private readonly ZObjectTable objectTable;
        private readonly ushort address;

        private readonly ZProperty[] properties;

        internal ZPropertyTable(ZObjectTable objectTable, ushort address)
        {
            this.objectTable = objectTable;
            this.address = address;

            properties = objectTable.ReadPropertyTableProperties(this);
        }

        public ushort[] GetShortNameZWords()
        {
            return objectTable.ReadShortName(address);
        }

        public bool Contains(int propNum)
        {
            foreach (var prop in properties)
            {
                if (prop.Number == propNum)
                {
                    return true;
                }
            }

            return false;
        }

        public ZProperty GetByNumber(int propNum)
        {
            foreach (var prop in properties)
            {
                if (prop.Number == propNum)
                {
                    return prop;
                }
            }

            return null;
        }

        public ushort Address
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
