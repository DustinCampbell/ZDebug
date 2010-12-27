using ZDebug.Core.Execution;
using ZDebug.Core.Utilities;
using System;

namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame stackFrame;

        public StackFrameViewModel(StackFrame stackFrame)
        {
            this.stackFrame = stackFrame;
        }

        public uint CallAddress
        {
            get { return stackFrame.CallAddress; }
        }

        public string ArgText
        {
            get 
            {
                return "(" + string.Join(", ", stackFrame.Arguments.ToArray().ConvertAll(arg => arg.ToString("x4"))) + ")";
            }
        }
    }
}
