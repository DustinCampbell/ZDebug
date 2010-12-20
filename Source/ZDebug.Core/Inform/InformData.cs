using ZDebug.Core.Basics;
using ZDebug.Core.Text;

namespace ZDebug.Core.Inform
{
    public sealed class InformData
    {
        private readonly Memory memory;
        private readonly MemoryMap memoryMap;
        private readonly int version;
        private readonly ZText ztext;

        public InformData(Memory memory, MemoryMap memoryMap, ZText ztext)
        {
            this.memory = memory;
            this.memoryMap = memoryMap;
            this.version = memory.ReadInformVersionNumber();
            this.ztext = ztext;
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

            return ztext.ZWordsAsString(propNameZWords, ZTextFlags.None);
        }
    }
}
