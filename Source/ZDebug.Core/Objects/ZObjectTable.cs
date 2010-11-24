using ZDebug.Core.Basics;

namespace ZDebug.Core.Objects
{
    public class ZObjectTable
    {
        private readonly Memory memory;
        private readonly int address;

        internal ZObjectTable(Memory memory)
        {
            this.memory = memory;
            this.address = memory.ReadObjectTableAddress();
        }

        public int Address
        {
            get { return address; }
        }
    }
}
