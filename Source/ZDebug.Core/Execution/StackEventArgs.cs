using System;

namespace ZDebug.Core.Execution
{
    public sealed class StackEventArgs : EventArgs
    {
        private readonly ushort value;

        public StackEventArgs(ushort value)
        {
            this.value = value;
        }

        public ushort Value
        {
            get { return value; }
        }
    }
}
