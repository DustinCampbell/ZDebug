using System.Collections;
using System.Collections.Generic;
using ZDebug.Core.Collections;

namespace ZDebug.Core.Basics
{
    public sealed class MemoryMap : IIndexedEnumerable<MemoryMapRegion>
    {
        private readonly Memory memory;
        private readonly List<MemoryMapRegion> regions;

        internal MemoryMap(Memory memory)
        {
            this.memory = memory;

            regions = new List<MemoryMapRegion>();
            regions.Add(new MemoryMapRegion("Header", 0, 0x3f));

            regions.Sort((r1, r2) => r1.Base.CompareTo(r2.Base));
        }

        public MemoryMapRegion this[int index]
        {
            get { return regions[index]; }
        }

        public int Count
        {
            get { return regions.Count; }
        }

        public IEnumerator<MemoryMapRegion> GetEnumerator()
        {
            foreach (var region in regions)
            {
                yield return region;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
