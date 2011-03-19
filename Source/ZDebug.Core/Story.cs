using System;
using System.IO;
using ZDebug.Core.Basics;
using ZDebug.Core.Dictionary;
using ZDebug.Core.Inform;
using ZDebug.Core.Instructions;
using ZDebug.Core.Interpreter;
using ZDebug.Core.Objects;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core
{
    public sealed class Story
    {
        private readonly byte[] memory;
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
        private readonly int mainRoutineAddress;

        private IInterpreter interpreter;

        private Story(byte[] memory)
        {
            this.memory = memory;
            this.version = Header.ReadVersion(memory);
            this.serialNumber = Header.ReadSerialNumber(memory);
            this.releaseNumber = Header.ReadReleaseNumber(memory);
            this.checksum = Header.ReadChecksum(memory);
            this.actualChecksum = Header.CalculateChecksum(memory);
            this.routinesOffset = Header.ReadRoutinesOffset(memory);
            this.stringsOffset = Header.ReadStringsOffset(memory);
            this.instructionCache = new InstructionCache((memory.Length - Header.ReadStaticMemoryBase(memory)) / 8);
            this.ztext = new ZText(memory);
            this.memoryMap = new MemoryMap(memory);
            this.informData = new InformData(memory, this.memoryMap, ztext);
            this.objectTable = new ZObjectTable(memory, ztext);
            this.globalVariablesTable = new GlobalVariablesTable(memory);
            this.dictionary = new ZDictionary(this, ztext);
            this.mainRoutineAddress = Header.ReadMainRoutineAddress(memory);

            RegisterInterpreter(new DefaultInterpreter());
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

        /// <summary>
        /// Registers a Z-Machine interpreter with this story.
        /// </summary>
        /// <param name="interpreter"></param>
        public void RegisterInterpreter(IInterpreter interpreter)
        {
            if (interpreter == null)
            {
                throw new ArgumentNullException("interpreter");
            }

            if (this.interpreter != null && !(this.interpreter is DefaultInterpreter))
            {
                throw new InvalidOperationException("Interpreter has already been registered.");
            }

            this.interpreter = interpreter;

            if (version >= 4)
            {
                memory.WriteByte(0x1e, (byte)interpreter.Target);
                memory.WriteByte(0x1f, interpreter.Version);
            }

            memory.WriteByte(0x32, interpreter.StandardRevisionMajorVersion);
            memory.WriteByte(0x33, interpreter.StandardRevisionMinorVersion);

            // Set various flags
            byte flags1 = memory.ReadByte(0x01);
            if (this.version <= 3)
            {
                // Only set this flag if the status line is NOT available
                flags1 = interpreter.SupportsStatusLine ? Bits.Clear(flags1, 4) : Bits.Set(flags1, 4);
                flags1 = interpreter.SupportsScreenSplitting ? Bits.Set(flags1, 5) : Bits.Set(flags1, 5);
                flags1 = interpreter.IsDefaultFontVariablePitch ? Bits.Set(flags1, 6) : Bits.Set(flags1, 6);
            }
            else // this.version >= 4
            {
                if (this.version >= 5)
                {
                    flags1 = interpreter.SupportsColor ? Bits.Set(flags1, 0) : Bits.Clear(flags1, 0);
                }

                if (this.version == 6)
                {
                    flags1 = interpreter.SupportsPictureDisplay ? Bits.Set(flags1, 1) : Bits.Clear(flags1, 1);
                    flags1 = interpreter.SupportsSoundEffects ? Bits.Set(flags1, 5) : Bits.Clear(flags1, 5);
                }

                flags1 = interpreter.SupportsBoldFont ? Bits.Set(flags1, 2) : Bits.Clear(flags1, 2);
                flags1 = interpreter.SupportsItalicFont ? Bits.Set(flags1, 3) : Bits.Clear(flags1, 3);
                flags1 = interpreter.SupportsFixedWidthFont ? Bits.Set(flags1, 4) : Bits.Clear(flags1, 4);
                flags1 = interpreter.SupportsTimedKeyboardInput ? Bits.Set(flags1, 7) : Bits.Clear(flags1, 7);
            }

            memory.WriteByte(0x01, flags1);

            ushort flags2 = memory.ReadWord(0x10);
            if (this.version >= 5)
            {
                flags2 = interpreter.SupportsPictureDisplay ? flags2 : Bits.Clear(flags2, 3);
                flags2 = interpreter.SupportsUndo ? flags2 : Bits.Clear(flags2, 4);
                flags2 = interpreter.SupportsMouse ? flags2 : Bits.Clear(flags2, 5);
                flags2 = interpreter.SupportsSoundEffects ? flags2 : Bits.Clear(flags2, 7);

                if (this.version == 6)
                {
                    flags2 = interpreter.SupportsMenus ? flags2 : Bits.Clear(flags2, 8);
                }
            }

            memory.WriteWord(0x10, flags2);
        }

        public byte[] Memory
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

        public ushort ActualChecksum
        {
            get { return actualChecksum; }
        }

        public MemoryMap MemoryMap
        {
            get { return memoryMap; }
        }

        public bool IsInformStory
        {
            get { return Header.IsInformStory(memory); }
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
            return new Story(stream.ReadFully());
        }
    }
}
