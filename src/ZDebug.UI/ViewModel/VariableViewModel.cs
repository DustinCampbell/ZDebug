
namespace ZDebug.UI.ViewModel;

internal class VariableViewModel : ViewModelBase
{
    private ushort value;
    private bool isModified;
    private bool visible;

    public VariableViewModel(ushort value)
    {
        this.value = value;
    }

    public bool IsModified
    {
        get => isModified;
        set
        {
            if (isModified != value)
            {
                isModified = value;
                PropertyChanged("IsModified");
            }
        }
    }

    public ushort Value
    {
        get => value;
        set
        {
            if (this.value != value)
            {
                this.value = value;
                PropertyChanged("Value");
            }
        }
    }

    public bool Visible
    {
        get => visible;
        set
        {
            if (visible != value)
            {
                visible = value;
                PropertyChanged("Visible");
            }
        }
    }
}
