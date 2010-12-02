using System;

namespace ZDebug.Core.Execution
{
    public sealed class StackFrameEventArgs : EventArgs
    {
        private readonly StackFrame oldFrame;
        private readonly StackFrame newFrame;

        public StackFrameEventArgs(StackFrame oldFrame, StackFrame newFrame)
        {
            this.oldFrame = oldFrame;
            this.newFrame = newFrame;
        }

        public StackFrame OldFrame
        {
            get { return oldFrame; }
        }

        public StackFrame NewFrame
        {
            get { return newFrame; }
        }
    }
}
