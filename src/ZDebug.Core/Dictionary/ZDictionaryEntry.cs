namespace ZDebug.Core.Dictionary;

public sealed class ZDictionaryEntry
{
    private readonly int address;
    private readonly int index;
    private readonly ushort[] zwords;
    private readonly string ztext;
    private readonly byte[] data;

    internal ZDictionaryEntry(int address, int index, ushort[] zwords, string ztext, byte[] data)
    {
        this.address = address;
        this.index = index;
        this.zwords = zwords;
        this.ztext = ztext;
        this.data = data;
    }

    public int Address => address;

    public int Index => index;

    public ushort[] ZWords => zwords;

    public string ZText => ztext;

    public byte[] Data => data;
}
