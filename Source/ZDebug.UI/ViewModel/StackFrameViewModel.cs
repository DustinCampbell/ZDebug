using ZDebug.Core.Execution;

namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly StackFrame currentFrame;
        private readonly int? callAddress;
        private bool isCurrent;

        public StackFrameViewModel(StackFrame currentFrame, int? callAddress)
        {
            this.currentFrame = currentFrame;
            this.callAddress = callAddress;
        }

        public int Address
        {
            get { return currentFrame.Address; }
        }

        public string CallAddressText
        {
            get
            {
                if (callAddress == null)
                {
                    return string.Empty;
                }

                return callAddress.Value.ToString("x4");
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
