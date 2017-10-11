namespace ZDebug.Core.Basics
{
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
            this.size = end - @base + 1;
        }

        public MemoryMapRegionKind Kind
        {
            get { return kind; }
        }

        public string Name
        {
            get { return name; }
        }

        public int Base
        {
            get { return @base; }
        }

        public int End
        {
            get { return end; }
        }

        public int Size
        {
            get { return size; }
        }
    }
}
