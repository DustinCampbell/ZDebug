using System;

namespace ZDebug.Core.Execution
{
    public class ProcessorSteppedEventArgs : EventArgs
    {
        private readonly int oldPC;
        private readonly int newPC;

        public ProcessorSteppedEventArgs(int oldPC, int newPC)
        {
            this.oldPC = oldPC;
            this.newPC = newPC;
        }

        public int OldPC
        {
            get { return oldPC; }
        }

        public int NewPC
        {
            get { return newPC; }
        }
    }
}
