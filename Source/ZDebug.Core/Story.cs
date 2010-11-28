using System.IO;
using ZDebug.Core.Basics;
using ZDebug.Core.Objects;

namespace ZDebug.Core
{
    public sealed class Story
    {
        private readonly Memory memory;
        private readonly byte version;
        private readonly int informVersion;

        private readonly MemoryMap memoryMap;
        private readonly ZObjectTable objectTable;

        private Story(Memory memory)
        {
            this.memory = memory;
            this.version = memory.ReadVersion();
            this.informVersion = memory.ReadInformVersionNumber();
            this.memoryMap = new MemoryMap(memory);
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

        public bool IsInformStory
        {
            get { return memory.IsInformStory(); }
        }

        public int InformVersion
        {
            get { return informVersion; }
        }

        public MemoryMap MemoryMap
        {
            get { return memoryMap; }
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
