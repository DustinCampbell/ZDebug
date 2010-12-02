using ZDebug.Core.Execution;

namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame currentFrame;
        private readonly StackFrame priorFrame;

        public StackFrameViewModel(StackFrame currentFrame, StackFrame priorFrame)
        {
            this.currentFrame = currentFrame;
            this.priorFrame = priorFrame;
        }
    }
}
