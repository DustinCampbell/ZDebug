using System;
using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    public sealed class LocalVariableChangedEventArgs : EventArgs
    {
        private readonly int index;
        private readonly Value oldValue;
        private readonly Value newValue;

        public LocalVariableChangedEventArgs(int index, Value oldValue, Value newValue)
        {
            this.index = index;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }

        public int Index
        {
            get { return index; }
        }

        public Value OldValue
        {
            get { return oldValue; }
        }

        public Value NewValue
        {
            get { return newValue; }
        }
    }
}
