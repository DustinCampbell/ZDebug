using System;

namespace ZDebug.Core.Basics
{
    public class MemoryChangedEventArgs : EventArgs
    {
        private readonly Memory memory;
        private readonly byte[] oldValues;
        private readonly byte[] newValues;
        private readonly int address;
        private readonly int length;

        public MemoryChangedEventArgs(Memory memory, int address, int length, byte[] oldValues, byte[] newValues)
        {
            this.memory = memory;
            this.oldValues = oldValues;
            this.newValues = newValues;
            this.address = address;
            this.length = length;
        }

        public Memory Memory
        {
            get { return memory; }
        }

        public byte[] OldValues
        {
            get { return oldValues; }
        }

        public byte[] NewValues
        {
            get { return newValues; }
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
