
namespace ZDebug.UI.ViewModel;

internal class MemoryLineViewModel : ViewModelBase
{
    private readonly int address;
    private readonly ushort[] values;

    public MemoryLineViewModel(int address, ushort[] values)
    {
        this.address = address;
        this.values = values;
    }

    public int Address => address;

    public ushort[] Values => values;

    public int ValueCount => values.Length;

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

    public ushort? Value1 => GetValue(0);

    public ushort? Value2 => GetValue(1);

    public ushort? Value3 => GetValue(2);

    public ushort? Value4 => GetValue(3);

    public ushort? Value5 => GetValue(4);

    public ushort? Value6 => GetValue(5);

    public ushort? Value7 => GetValue(6);

    public ushort? Value8 => GetValue(7);
}
