
namespace ZDebug.UI.ViewModel
{
    internal abstract partial class DisassemblyLineViewModel : ViewModelBase
    {
        private bool hasBreakpoint;
        private bool hasIP;

        public bool HasBreakpoint
        {
            get { return hasBreakpoint; }
            set
            {
                if (hasBreakpoint != value)
                {
                    hasBreakpoint = value;
                    PropertyChanged("HasBreakpoint");
                }
            }
        }

        public bool HasIP
        {
            get { return hasIP; }
            set
            {
                if (hasIP != value)
                {
                    hasIP = value;
                    PropertyChanged("HasIP");
                }
            }
        }

        public DisassemblyLineState State
        {
            get { return DisassemblyLineState.None; }
        }

        public string ToolTip
        {
            get { return string.Empty; }
        }
    }
}
