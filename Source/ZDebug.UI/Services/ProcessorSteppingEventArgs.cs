using System;

namespace ZDebug.UI.Services
{
    public class ProcessorSteppingEventArgs : EventArgs
    {
        private readonly int oldPC;

        public ProcessorSteppingEventArgs(int oldPC)
        {
            this.oldPC = oldPC;
        }

        public int OldPC
        {
            get { return oldPC; }
        }
    }
}
