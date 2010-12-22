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
        private readonly IntegerMap<ZProperty> numberToPropertyMap;

        internal ZPropertyTable(ZObjectTable objectTable, ushort address)
        {
            this.objectTable = objectTable;
            this.address = address;

            properties = objectTable.ReadPropertyTableProperties(this);
            numberToPropertyMap = new IntegerMap<ZProperty>(properties.Length);

            foreach (var prop in properties)
            {
                numberToPropertyMap.Add(prop.Number, prop);
            }
        }

        public ushort[] GetShortNameZWords()
        {
            return objectTable.ReadShortName(address);
        }

        public bool Contains(int propNum)
        {
            return numberToPropertyMap.Contains(propNum);
        }

        public ZProperty GetByNumber(int propNum)
        {
            ZProperty prop;
            if (numberToPropertyMap.TryGetValue(propNum, out prop))
            {
                return prop;
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
