
namespace ZDebug.UI.ViewModel
{
    internal class VariableViewModel : ViewModelBase
    {
        private ushort value;
        private bool isModified;
        private bool isFrozen;
        private ushort frozenValue;
        private bool visible;

        public VariableViewModel(ushort value)
        {
            this.value = value;
        }

        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    if (!isFrozen)
                    {
                        PropertyChanged("Visible");
                    }
                }
            }
        }

        public ushort Value
        {
            get
            {
                if (isFrozen)
                {
                    return frozenValue;
                }
                else
                {
                    return value;
                }
            }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    if (!isFrozen)
                    {
                        PropertyChanged("Value");
                    }
                }
            }
        }

        public bool IsModified
        {
            get { return isModified; }
            set
            {
                if (isModified != value)
                {
                    isModified = value;
                    if (!isFrozen)
                    {
                        PropertyChanged("IsModified");
                    }
                }
            }
        }

        public bool IsFrozen
        {
            get { return isFrozen; }
            set
            {
                if (isFrozen != value)
                {
                    isFrozen = value;
                    PropertyChanged("IsFrozen");

                    if (isFrozen)
                    {
                        frozenValue = this.value;
                    }
                    else
                    {
                        AllPropertiesChanged();
                    }
                }
            }
        }
    }
}
