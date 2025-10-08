namespace ZDebug.Core.Basics;

public sealed record MemoryMapRegion(MemoryMapRegionKind Kind, string Name, int Base, int End)
{
    public int Size => End - Base + 1;
}
