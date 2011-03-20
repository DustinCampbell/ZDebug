using System.Collections;
using System.Collections.Generic;
using ZDebug.Core.Collections;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Basics
{
    public sealed class MemoryMap : IIndexedEnumerable<MemoryMapRegion>
    {
        private readonly byte[] memory;
        private readonly List<MemoryMapRegion> regions;
        private readonly Dictionary<MemoryMapRegionKind, MemoryMapRegion> kindToRegionMap;

        internal MemoryMap(byte[] memory)
        {
            this.memory = memory;

            regions = new List<MemoryMapRegion>();
            kindToRegionMap = new Dictionary<MemoryMapRegionKind, MemoryMapRegion>();

            AddHeaderRegions(memory);
            AddAbbreviationRegions(memory);
            AddDictionaryRegion(memory);
            AddObjectTableRegions(memory);
            AddInformTables(memory);

            regions.Sort((r1, r2) => r1.Base.CompareTo(r2.Base));
        }

        private void AddRegion(MemoryMapRegionKind kind, string name, int @base, int end)
        {
            var region = new MemoryMapRegion(kind, name, @base, end);
            regions.Add(region);
            kindToRegionMap.Add(kind, region);
        }

        private void AddHeaderRegions(byte[] memory)
        {
            AddRegion(MemoryMapRegionKind.Header, "Header", 0, 0x3f);

            var headerExtensionBase = Header.ReadHeaderExtensionTableAddress(memory);
            if (headerExtensionBase > 0)
            {
                var headerExtensionSize = memory.ReadWord(headerExtensionBase);
                var headerExtensionEnd = headerExtensionBase + 2 + (headerExtensionSize * 2) - 1;
                AddRegion(MemoryMapRegionKind.HeaderExtensionTable, "Header extension table", headerExtensionBase, headerExtensionEnd);

                if (headerExtensionSize > 2)
                {
                    var unicodeTableBase = memory.ReadWord(headerExtensionBase + 6);
                    if (unicodeTableBase > 0)
                    {
                        var unicodeTableEnd = unicodeTableBase + (memory.ReadByte(unicodeTableBase) * 2);
                        AddRegion(MemoryMapRegionKind.UnicodeTable, "Unicode table", unicodeTableBase, unicodeTableEnd);
                    }
                }
            }
        }

        private void AddAbbreviationRegions(byte[] memory)
        {
            var version = Header.ReadVersion(memory);
            if (version == 1) // V1 did not support abbreviations
            {
                return;
            }

            var tableBase = Header.ReadAbbreviationsTableAddress(memory);
            var count = version == 2 ? 32 : 96;
            var tableEnd = tableBase + (count * 2) - 1;

            AddRegion(MemoryMapRegionKind.AbbreviationPointerTable, "Abbreviation pointer table", tableBase, tableEnd);

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

            AddRegion(MemoryMapRegionKind.AbbreviationData, "Abbreviation data", dataBase, dataEnd);
        }

        private void AddDictionaryRegion(byte[] memory)
        {
            var dictionaryBase = Header.ReadDictionaryAddress(memory);

            var reader = new MemoryReader(memory, dictionaryBase);
            var separatorCount = reader.NextByte();
            reader.Skip(separatorCount);

            var entrySize = reader.NextByte();
            var entryCount = reader.NextWord();

            var dictionaryEnd = (reader.Address + (entrySize * entryCount)) - 1;

            AddRegion(MemoryMapRegionKind.Dictionary, "Dictionary", dictionaryBase, dictionaryEnd);
        }

        private void AddObjectTableRegions(byte[] memory)
        {
            var objectTableBase = Header.ReadObjectTableAddress(memory);
            var objectTableEnd = 0;
            var objectDataBase = 0;
            var objectDataEnd = 0;

            var version = Header.ReadVersion(memory);
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
            var reader = new MemoryReader(memory, objectDataEnd);
            reader.SkipShortName();
            reader.SkipProperties(version);

            objectDataEnd = reader.Address;
            objectDataEnd--;

            AddRegion(MemoryMapRegionKind.ObjectTable, "Object table", objectTableBase, objectTableEnd);
            AddRegion(MemoryMapRegionKind.PropertyData, "Property data", objectDataBase, objectDataEnd);
        }

        private void AddInformTables(byte[] memory)
        {
            if (!Header.IsInformStory(memory))
            {
                return;
            }

            var informVersion = Header.ReadInformVersionNumber(memory);
            if (informVersion < 600)
            {
                return;
            }

            var propertyDataRegion = kindToRegionMap[MemoryMapRegionKind.PropertyData];
            var classNumbersBase = propertyDataRegion.End + 1;

            var reader = new MemoryReader(memory, classNumbersBase);

            while (reader.NextWord() != 0)
                ;

            var classNumbersEnd = reader.Address - 1;

            AddRegion(MemoryMapRegionKind.ClassPrototypeObjectNumbers, "Class prototype object numbers", classNumbersBase, classNumbersEnd);

            var propertyNamesBase = reader.Address;
            var propertyCount = reader.NextWord() - 1;
            reader.Skip(propertyCount * 2);
            var propertyNamesEnd = reader.Address - 1;

            AddRegion(MemoryMapRegionKind.PropertyNamesTable, "Property names table", propertyNamesBase, propertyNamesEnd);

            if (informVersion < 610)
            {
                return;
            }

            var attributeNamesBase = reader.Address;
            var attributeNamesEnd = (attributeNamesBase + (48 * 2)) - 1;

            AddRegion(MemoryMapRegionKind.AttributeNamesTable, "Attribute names table", attributeNamesBase, attributeNamesEnd);
        }

        public MemoryMapRegion this[int index]
        {
            get { return regions[index]; }
        }

        public MemoryMapRegion this[MemoryMapRegionKind kind]
        {
            get { return kindToRegionMap[kind]; }
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
