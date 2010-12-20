using ZDebug.Core.Basics;
using ZDebug.Core.Text;

namespace ZDebug.Core.Inform
{
    public sealed class InformData
    {
        private readonly Memory memory;
        private readonly MemoryMap memoryMap;
        private readonly int version;

        public InformData(Memory memory, MemoryMap memoryMap)
        {
            this.memory = memory;
            this.memoryMap = memoryMap;
            this.version = memory.ReadInformVersionNumber();
        }

        public int Version
        {
            get { return version; }
        }

        public string GetPropertyName(int propNum)
        {
            var address = memoryMap[MemoryMapRegionKind.PropertyNamesTable].Base + (propNum * 2);
            var propNamePackedAddress = memory.ReadWord(address);
            var propNameAddress = memory.UnpackStringAddress(propNamePackedAddress);

            var propNameZWords = memory.ReadZWords(propNameAddress);

            return ZText.ZWordsAsString(propNameZWords, ZTextFlags.None, memory);
        }
    }
}
