using System;

namespace ZDebug.UI.Services
{
    public class DebuggerStateChangedEventArgs : EventArgs
    {
        private readonly DebuggerState oldState;
        private readonly DebuggerState newState;

        public DebuggerStateChangedEventArgs(DebuggerState oldState, DebuggerState newState)
        {
            this.oldState = oldState;
            this.newState = newState;
        }

        public DebuggerState OldState
        {
            get { return oldState; }
        }

        public DebuggerState NewState
        {
            get { return newState; }
        }
    }
}
