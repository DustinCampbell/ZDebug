namespace ZDebug.UI.ViewModel
{
    internal sealed class StackFrameViewModel : ViewModelBase
    {
        private readonly int address;
        private readonly int? callAddress;
        private bool isCurrent;

        public StackFrameViewModel(int address, int? callAddress)
        {
            this.address = address;
            this.callAddress = callAddress;
        }

        public int Address
        {
            get { return address; }
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
