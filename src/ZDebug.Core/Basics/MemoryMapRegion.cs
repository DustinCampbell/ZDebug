namespace ZDebug.Core.Basics;

public sealed class MemoryMapRegion
{
    private readonly MemoryMapRegionKind kind;
    private readonly string name;
    private readonly int @base;
    private readonly int end;
    private readonly int size;

    internal MemoryMapRegion(MemoryMapRegionKind kind, string name, int @base, int end)
    {
        this.kind = kind;
        this.name = name;
        this.@base = @base;
        this.end = end;
        size = end - @base + 1;
    }

    public MemoryMapRegionKind Kind => kind;

    public string Name => name;

    public int Base => @base;

    public int End => end;

    public int Size => size;
}
