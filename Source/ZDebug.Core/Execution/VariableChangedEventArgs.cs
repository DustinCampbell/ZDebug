using System;

namespace ZDebug.Core.Execution
{
    public sealed class VariableChangedEventArgs : EventArgs
    {
        private readonly int index;
        private readonly ushort oldValue;
        private readonly ushort newValue;

        public VariableChangedEventArgs(int index, ushort oldValue, ushort newValue)
        {
            this.index = index;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public int Index
        {
            get { return index; }
        }

        public ushort OldValue
        {
            get { return oldValue; }
        }

        public ushort NewValue
        {
            get { return newValue; }
        }
    }
}
