
namespace ZDebug.Core.Objects
{
    public class ZObject
    {
        private readonly ZObjectTable objectTable;
        private readonly int address;
        private readonly int number;

        internal ZObject(ZObjectTable objectTable, int address, int number)
        {
            this.objectTable = objectTable;
            this.address = address;
            this.number = number;
        }

        public int Address
        {
            get { return address; }
        }

        public int Number
        {
            get { return number; }
        }
    }
}
