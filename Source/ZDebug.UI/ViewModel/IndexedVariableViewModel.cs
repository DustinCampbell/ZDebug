namespace ZDebug.UI.ViewModel
{
    internal sealed class IndexedVariableViewModel : VariableViewModel
    {
        private readonly int index;

        public IndexedVariableViewModel(int index, ushort value)
            : base(value)
        {
            this.index = index;
        }

        public int Index
        {
            get { return index; }
        }
    }
}
