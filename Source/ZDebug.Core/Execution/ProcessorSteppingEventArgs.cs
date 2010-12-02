using System;

namespace ZDebug.Core.Execution
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
