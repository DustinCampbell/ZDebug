using System;

namespace ZDebug.Core.Basics
{
    public sealed class MemoryEventArgs : EventArgs
    {
        private readonly Memory memory;
        private readonly int address;
        private readonly int length;

        public MemoryEventArgs(Memory memory, int address, int length)
        {
            this.memory = memory;
            this.address = address;
            this.length = length;
        }

        public Memory Memory
        {
            get { return memory; }
        }

        public int Address
        {
            get { return address; }
        }

        public int Length
        {
            get { return length; }
        }
    }
}
