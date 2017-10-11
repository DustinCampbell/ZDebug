using System;

namespace ZDebug.Core.Execution
{
    public sealed class StackFrameEventArgs : EventArgs
    {
        private readonly int address;
        private readonly int previousAddress;

        public StackFrameEventArgs(int address, int previousAddress)
        {
            this.address = address;
            this.previousAddress = previousAddress;
        }

        public int Address
        {
            get { return address; }
        }

        public int PreviousAddress
        {
            get { return previousAddress; }
        }
    }
}
