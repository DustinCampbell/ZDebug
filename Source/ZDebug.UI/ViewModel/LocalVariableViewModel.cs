using ZDebug.Core.Instructions;

namespace ZDebug.UI.ViewModel
{
    internal sealed class LocalVariableViewModel : ViewModelBase
    {
        private readonly int index;
        private Value value;

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
            get { return value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    PropertyChanged("Value");
                }
            }
        }
    }
}
