
namespace ZDebug.UI.ViewModel
{
    internal class MemoryLineViewModel : ViewModelBase
    {
        private readonly int address;
        private readonly ushort[] values;

        public MemoryLineViewModel(int address, ushort[] values)
        {
            this.address = address;
            this.values = values;
        }

        public int Address
        {
            get { return address; }
        }

        public ushort[] Values
        {
            get { return values; }
        }

        public int ValueCount
        {
            get { return values.Length; }
        }

        private ushort? GetValue(int index)
        {
            if (values.Length > index)
            {
                return values[index];
            }
            else
            {
                return null;
            }
        }

        public ushort? Value1
        {
            get { return GetValue(0); }
        }

        public ushort? Value2
        {
            get { return GetValue(1); }
        }

        public ushort? Value3
        {
            get { return GetValue(2); }
        }

        public ushort? Value4
        {
            get { return GetValue(3); }
        }

        public ushort? Value5
        {
            get { return GetValue(4); }
        }

        public ushort? Value6
        {
            get { return GetValue(5); }
        }

        public ushort? Value7
        {
            get { return GetValue(6); }
        }

        public ushort? Value8
        {
            get { return GetValue(7); }
        }
    }
}
