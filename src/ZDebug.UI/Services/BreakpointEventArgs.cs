using System;

namespace ZDebug.UI.Services
{
    public sealed class BreakpointEventArgs : EventArgs
    {
        private readonly int address;

        public BreakpointEventArgs(int address)
        {
            this.address = address;
        }

        public int Address
        {
            get { return address; }
        }
    }
}
