using System;

namespace ZDebug.Core.Basics
{
    public class MemoryChangedEventArgs : EventArgs
    {
        private readonly Memory memory;
        private readonly byte[] oldValues;
        private readonly byte[] newValues;
        private readonly int index;
        private readonly int length;

        public MemoryChangedEventArgs(Memory memory, int index, int length, byte[] oldValues, byte[] newValues)
        {
            this.memory = memory;
            this.oldValues = oldValues;
            this.newValues = newValues;
            this.index = index;
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

        public int Index
        {
            get { return index; }
        }

        public int Length
        {
            get { return length; }
        }
    }
}
