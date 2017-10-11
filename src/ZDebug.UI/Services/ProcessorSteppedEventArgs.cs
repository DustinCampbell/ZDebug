using System;

namespace ZDebug.UI.Services
{
    public class SteppedEventArgs : EventArgs
    {
        private readonly int oldPC;
        private readonly int newPC;

        public SteppedEventArgs(int oldPC, int newPC)
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
