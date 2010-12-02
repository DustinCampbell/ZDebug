using System.IO;
using ZDebug.Core.Basics;
using ZDebug.Core.Execution;
using ZDebug.Core.Inform;
using ZDebug.Core.Instructions;
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
        private readonly RoutineTable routineTable;
        private readonly Processor processor;

        private Story(Memory memory)
        {
            this.memory = memory;
            this.version = memory.ReadVersion();
            this.memoryMap = new MemoryMap(memory);
            this.informData = new InformData(memory, this.memoryMap);
            this.objectTable = new ZObjectTable(memory);
            this.routineTable = new RoutineTable(memory);
            this.processor = new Processor(this);
        }

        private Story(byte[] bytes)
            : this(new Memory(bytes))
        {
        }

        private Story(Stream stream)
            : this(new Memory(stream))
        {
        }

        public int UnpackRoutineAddress(ushort byteAddress)
        {
            return memory.UnpackRoutineAddress(byteAddress);
        }

        public int UnpackStringAddress(ushort byteAddress)
        {
            return memory.UnpackStringAddress(byteAddress);
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

        public RoutineTable RoutineTable
        {
            get { return routineTable; }
        }

        public Processor Processor
        {
            get { return processor; }
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
