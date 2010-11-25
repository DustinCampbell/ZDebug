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

            AddAbbreviationRegions(memory);

            regions.Sort((r1, r2) => r1.Base.CompareTo(r2.Base));
        }

        private void AddAbbreviationRegions(Memory memory)
        {
            var version = memory.ReadVersion();
            if (version == 1) // V1 did not support abbreviations
            {
                return;
            }

            var tableBase = memory.ReadAbbreviationsTableAddress();
            var count = version == 2 ? 32 : 96;
            var tableEnd = tableBase + (count * 2) - 1;

            regions.Add(new MemoryMapRegion("Abbreviation pointer table", tableBase, tableEnd));

            var dataBase = 0;
            var dataEnd = 0;

            for (int i = 0; i < count; i++)
            {
                var address = memory.ReadWord(tableBase + (i * 2)) * 2;

                if (dataBase == 0 || address < dataBase)
                {
                    dataBase = address;
                }

                if (dataEnd == 0 || address > dataEnd)
                {
                    dataEnd = address;
                }
            }

            // scan last string in abbreviation data to get end...
            while (true)
            {
                var zword = memory.ReadWord(dataEnd);
                dataEnd += 2;

                if ((zword & 0x8000) != 0)
                {
                    break;
                }
            }

            dataEnd--;

            regions.Add(new MemoryMapRegion("Abbreviation data", dataBase, dataEnd));
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
