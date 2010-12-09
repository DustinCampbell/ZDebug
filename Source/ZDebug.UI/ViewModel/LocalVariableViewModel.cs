using ZDebug.Core.Instructions;

namespace ZDebug.UI.ViewModel
{
    internal sealed class LocalVariableViewModel : ViewModelBase
    {
        private readonly int index;
        private Value value;
        private bool isModified;
        private bool isFrozen;
        private Value frozenValue;

        public LocalVariableViewModel(int index, Value value)
        {
            this.index = index;
            this.value = value;
        }

        public int Index
        {
            get { return index; }
        }

        public Value Value
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
                    frozenValue = this.value;
                    PropertyChanged("IsFrozen");
                    PropertyChanged("Value");
                }
            }
        }
    }
}
