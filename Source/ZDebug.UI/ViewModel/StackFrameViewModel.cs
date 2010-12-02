using ZDebug.Core.Execution;

namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame currentFrame;
        private readonly StackFrame priorFrame;
        private bool isCurrent;

        public StackFrameViewModel(StackFrame currentFrame, StackFrame priorFrame)
        {
            this.currentFrame = currentFrame;
            this.priorFrame = priorFrame;
        }

        public int Address
        {
            get { return currentFrame.Address; }
        }

        public bool IsCurrent
        {
            get { return isCurrent; }
            set
            {
                if (isCurrent != value)
                {
                    isCurrent = value;
                    PropertyChanged("IsCurrent");
                }
            }
        }
    }
}
