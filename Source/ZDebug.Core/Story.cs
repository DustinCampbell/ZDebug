using System.IO;
using ZDebug.Core.Basics;
using ZDebug.Core.Inform;
using ZDebug.Core.Objects;

namespace ZDebug.Core
{
    public sealed class Story
    {
        private readonly Memory memory;
        private readonly byte version;

        private readonly MemoryMap memoryMap;
        private readonly InformData informData;
        private readonly ZObjectTable objectTable;

        private Story(Memory memory)
        {
            this.memory = memory;
            this.version = memory.ReadVersion();
            this.memoryMap = new MemoryMap(memory);
            this.informData = new InformData(memory);
            this.objectTable = new ZObjectTable(memory);
        }

        private Story(byte[] bytes)
            : this(new Memory(bytes))
        {
        }

        private Story(Stream stream)
            : this(new Memory(stream))
        {
        }

        public Memory Memory
        {
            get { return memory; }
        }

        public byte Version
        {
            get { return version; }
        }

        public MemoryMap MemoryMap
        {
            get { return memoryMap; }
        }

        public bool IsInformStory
        {
            get { return memory.IsInformStory(); }
        }

        public InformData InformData
        {
            get { return informData; }
        }

        public ZObjectTable ObjectTable
        {
            get { return objectTable; }
        }

        public static Story FromBytes(byte[] bytes)
        {
            return new Story(bytes);
        }

        public static Story FromStream(Stream stream)
        {
            return new Story(stream);
        }
    }
}
