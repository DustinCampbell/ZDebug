using ZDebug.Core.Basics;

namespace ZDebug.Core.Objects
{
    public class ZPropertyTable
    {
        private readonly Memory memory;
        private readonly int address;

        internal ZPropertyTable(Memory memory, int address)
        {
            this.memory = memory;
            this.address = address;
        }

        public ushort[] GetShortName()
        {
            return memory.ReadShortName(address);
        }

        public int Address
        {
            get { return address; }
        }
    }
}
