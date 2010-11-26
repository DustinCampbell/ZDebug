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

            AddHeaderRegions(memory);
            AddAbbreviationRegions(memory);
            AddDictionaryRegion(memory);
            AddObjectTableRegions(memory);

            regions.Sort((r1, r2) => r1.Base.CompareTo(r2.Base));
        }

        private void AddHeaderRegions(Memory memory)
        {
            regions.Add(new MemoryMapRegion("Header", 0, 0x3f));

            var headerExtensionBase = memory.ReadHeaderExtensionTableAddress();
            if (headerExtensionBase > 0)
            {
                var headerExtensionSize = memory.ReadWord(headerExtensionBase);
                var headerExtensionEnd = headerExtensionBase + 2 + (headerExtensionSize * 2) - 1;
                regions.Add(new MemoryMapRegion("Header extension table", headerExtensionBase, headerExtensionEnd));

                if (headerExtensionSize > 2)
                {
                    var unicodeTableBase = memory.ReadWord(headerExtensionBase + 6);
                    if (unicodeTableBase > 0)
                    {
                        var unicodeTableEnd = unicodeTableBase + (memory.ReadByte(unicodeTableBase) * 2);
                        regions.Add(new MemoryMapRegion("Unicode table", unicodeTableBase, unicodeTableEnd));
                    }
                }
            }
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

        private void AddDictionaryRegion(Memory memory)
        {
            var dictionaryBase = memory.ReadDictionaryAddress();

            var reader = memory.CreateReader(dictionaryBase);
            var separatorCount = reader.NextByte();
            reader.Skip(separatorCount);

            var entrySize = reader.NextByte();
            var entryCount = reader.NextWord();

            var dictionaryEnd = (reader.Address + (entrySize * entryCount)) - 1;

            regions.Add(new MemoryMapRegion("Dictionary", dictionaryBase, dictionaryEnd));
        }

        private void AddObjectTableRegions(Memory memory)
        {
            var objectTableBase = memory.ReadObjectTableAddress();
            var objectTableEnd = 0;
            var objectDataBase = 0;
            var objectDataEnd = 0;

            var version = memory.ReadVersion();
            var entrySize = ObjectHelpers.GetEntrySize(version);
            var propertyTableOffset = ObjectHelpers.GetPropertyTableAddressOffset(version);

            int objectAddress = objectTableBase + ObjectHelpers.GetPropertyDefaultsTableSize(version);
            while (objectDataBase == 0 || objectAddress < objectDataBase)
            {
                if (objectDataBase == 0 || objectAddress < objectDataBase)
                {
                    var propertyTable = memory.ReadWord(objectAddress + propertyTableOffset);

                    if (objectDataBase == 0 || propertyTable < objectDataBase)
                    {
                        objectDataBase = propertyTable;
                    }

                    if (objectDataEnd == 0 || propertyTable > objectDataEnd)
                    {
                        objectDataEnd = propertyTable;
                    }
                }

                objectAddress += entrySize;
            }

            objectTableEnd = objectAddress - 1;

            // skip last property table to get end...
            var reader = memory.CreateReader(objectDataEnd);
            reader.SkipShortName();
            reader.SkipProperties(version);

            objectDataEnd = reader.Address;
            objectDataEnd--;

            regions.Add(new MemoryMapRegion("Object table", objectTableBase, objectTableEnd));
            regions.Add(new MemoryMapRegion("Property data", objectDataBase, objectDataEnd));
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
