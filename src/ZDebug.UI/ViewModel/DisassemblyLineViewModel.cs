namespace ZDebug.UI.ViewModel;

internal abstract partial class DisassemblyLineViewModel : ViewModelBase
{
    private bool hasBreakpoint;
    private bool hasIP;
    private bool showBlankBefore;
    private bool showBlankAfter;
    private DisassemblyLineState state;
    private object toolTip;

    public bool HasBreakpoint
    {
        get => hasBreakpoint;
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
        get => hasIP;
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
        get => state;
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
        get => toolTip;
        set
        {
            if (toolTip != value)
            {
                toolTip = value;
                PropertyChanged("ToolTip");
            }
        }
    }

    public bool ShowBlankBefore
    {
        get => showBlankBefore;
        set
        {
            if (showBlankBefore != value)
            {
                showBlankBefore = value;
                PropertyChanged("ShowBlankBefore");
            }
        }
    }

    public bool ShowBlankAfter
    {
        get => showBlankAfter;
        set
        {
            if (showBlankAfter != value)
            {
                showBlankAfter = value;
                PropertyChanged("ShowBlankAfter");
            }
        }
    }
}
