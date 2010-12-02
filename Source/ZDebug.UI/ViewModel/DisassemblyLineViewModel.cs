
namespace ZDebug.UI.ViewModel
{
    internal abstract partial class DisassemblyLineViewModel : ViewModelBase
    {
        private bool hasBreakpoint;
        private bool hasIP;
        private DisassemblyLineState state;
        private object toolTip;

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
            get { return state; }
            set
            {
                if (state != value)
                {
                    state = value;
                    PropertyChanged("State");
                }
            }
        }

        public object ToolTip
        {
            get { return toolTip; }
            set
            {
                if (toolTip != value)
                {
                    toolTip = value;
                    PropertyChanged("ToolTip");
                }
            }
        }
    }
}
