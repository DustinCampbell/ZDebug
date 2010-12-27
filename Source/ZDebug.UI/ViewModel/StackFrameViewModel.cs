using ZDebug.Core.Execution;
namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame stackFrame;
        private bool isCurrent;

        public StackFrameViewModel(StackFrame stackFrame)
        {
            this.stackFrame = stackFrame;
        }

        public uint Address
        {
            get { return stackFrame.CallAddress; }
        }

        public string CallAddressText
        {
            get
            {
                if (stackFrame.ReturnAddress == 0)
                {
                    return string.Empty;
                }

                return stackFrame.ReturnAddress.ToString("x4");
            }
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
