using System;
using System.IO;
using ZDebug.Core.Basics;
using ZDebug.Core.Dictionary;
using ZDebug.Core.Execution;
using ZDebug.Core.Inform;
using ZDebug.Core.Instructions;
using ZDebug.Core.Objects;
using ZDebug.Core.Text;

namespace ZDebug.Core
{
    public sealed class Story
    {
        private readonly Memory memory;
        private readonly byte version;
        private readonly int serialNumber;
        private readonly ushort releaseNumber;
        private readonly ushort checksum;
        private readonly ushort actualChecksum;
        private readonly ushort routinesOffset;
        private readonly ushort stringsOffset;

        private readonly InstructionCache instructionCache;

        private readonly ZText ztext;
        private readonly MemoryMap memoryMap;
        private readonly InformData informData;
        private readonly ZObjectTable objectTable;
        private readonly GlobalVariablesTable globalVariablesTable;
        private readonly ZDictionary dictionary;
        private readonly Processor processor;
        private readonly int mainRoutineAddress;

        private Story(Memory memory)
        {
            this.memory = memory;
            this.version = memory.ReadVersion();
            this.serialNumber = memory.ReadSerialNumber();
            this.releaseNumber = memory.ReadReleaseNumber();
            this.checksum = memory.ReadChecksum();
            this.actualChecksum = memory.CalculateChecksum();
            this.routinesOffset = memory.ReadRoutinesOffset();
            this.stringsOffset = memory.ReadStringsOffset();
            this.instructionCache = new InstructionCache((memory.Size - memory.ReadStaticMemoryBase()) / 8);
            this.ztext = new ZText(memory);
            this.memoryMap = new MemoryMap(memory);
            this.informData = new InformData(memory, this.memoryMap, ztext);
            this.objectTable = new ZObjectTable(memory, ztext);
            this.globalVariablesTable = new GlobalVariablesTable(memory);
            this.dictionary = new ZDictionary(this, ztext);
            this.processor = new Processor(this, ztext);
            this.mainRoutineAddress = memory.ReadMainRoutineAddress();

            // write interpreter number
            if (version >= 4)
            {
                memory.WriteByte(0x1e, 6); // MS-DOS
                memory.WriteByte(0x1f, 65); // A
            }

            // write standard revision number
            memory.WriteByte(0x32, 1);
            memory.WriteByte(0x33, 0);
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
            switch (version)
            {
                case 1:
                case 2:
                case 3:
                    return byteAddress * 2;
                case 4:
                case 5:
                    return byteAddress * 4;
                case 6:
                case 7:
                    return (byteAddress * 4) + (routinesOffset * 8);
                case 8:
                    return byteAddress * 8;
                default:
                    throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public int UnpackStringAddress(ushort byteAddress)
        {
            switch (version)
            {
                case 1:
                case 2:
                case 3:
                    return byteAddress * 2;
                case 4:
                case 5:
                    return byteAddress * 4;
                case 6:
                case 7:
                    return (byteAddress * 4) + (stringsOffset * 8);
                case 8:
                    return byteAddress * 8;
                default:
                    throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public Memory Memory
        {
            get { return memory; }
        }

        public ZText ZText
        {
            get { return ztext; }
        }

        public byte Version
        {
            get { return version; }
        }

        public int SerialNumber
        {
            get { return serialNumber; }
        }

        public ushort ReleaseNumber
        {
            get { return releaseNumber; }
        }

        public ushort Checksum
        {
            get { return checksum; }
        }

        internal ushort ActualChecksum
        {
            get { return actualChecksum; }
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

        public GlobalVariablesTable GlobalVariablesTable
        {
            get { return globalVariablesTable; }
        }

        public ZDictionary Dictionary
        {
            get { return dictionary; }
        }

        public Processor Processor
        {
            get { return processor; }
        }

        public int MainRoutineAddress
        {
            get { return mainRoutineAddress; }
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
