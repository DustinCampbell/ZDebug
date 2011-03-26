using System;

namespace ZDebug.UI.Services
{
    public class SteppingEventArgs : EventArgs
    {
        private readonly int oldPC;

        public SteppingEventArgs(int oldPC)
        {
            this.oldPC = oldPC;
        }

        public int OldPC
        {
            get { return oldPC; }
        }
    }
}
