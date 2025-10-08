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

        public int OldPC => oldPC;

        public int NewPC => newPC;
    }
}
