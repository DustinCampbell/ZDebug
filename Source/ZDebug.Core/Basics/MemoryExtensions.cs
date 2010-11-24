using System;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    /// <summary>
    /// Extension methods for reading and writing Z-Machine memory.
    /// </summary>
    internal static class MemoryExtensions
    {
        private const int VersionIndex = 0x00;
        private const int Flags1Index = 0x01;
        private const int ReleaseNumberIndex = 0x02;
        private const int HighMemoryBaseIndex = 0x04;
        private const int InitialPCIndex = 0x06;
        private const int DictionaryAddressIndex = 0x08;
        private const int ObjectTableAddressIndex = 0x0a;
        private const int GlobalVariableTableAddressIndex = 0x0c;
        private const int StaticMemoryBaseIndex = 0x0e;
        private const int Flags2Index = 0x10;
        private const int SerialNumberIndex = 0x12;
        private const int AbbreviationTableAddressIndex = 0x18;
        private const int FileSizeIndex = 0x1a;
        private const int ChecksumIndex = 0x1c;
        private const int RoutinesOffsetIndex = 0x28;
        private const int StringsOffsetIndex = 0x2a;
        private const int TerminatingCharactersTableAddressIndex = 0x2e;
        private const int AlphabetTableAddressIndex = 0x34;
        private const int HeaderExtensionTableAddressIndex = 0x36;
        private const int InformVersionNumberIndex = 0x3C;

        public static string ReadAsciiString(this Memory memory, int index, int length)
        {
            var chars = memory.ReadBytes(index, length).Select(b => (char)b);
            return new string(chars);
        }

        public static byte ReadVersion(this Memory memory)
        {
            return memory.ReadByte(VersionIndex);
        }

        public static byte ReadFlags1(this Memory memory)
        {
            return memory.ReadByte(Flags1Index);
        }
        public static ushort ReadReleaseNumber(this Memory memory)
        {
            return memory.ReadWord(ReleaseNumberIndex);
        }

        public static ushort ReadHighMemoryBase(this Memory memory)
        {
            return memory.ReadWord(HighMemoryBaseIndex);
        }

        public static ushort ReadInitialPC(this Memory memory)
        {
            return memory.ReadWord(InitialPCIndex);
        }

        public static ushort ReadDictionaryAddress(this Memory memory)
        {
            return memory.ReadWord(DictionaryAddressIndex);
        }

        public static ushort ReadObjectTableAddress(this Memory memory)
        {
            return memory.ReadWord(ObjectTableAddressIndex);
        }

        public static ushort ReadGlobalVariableTableAddress(this Memory memory)
        {
            return memory.ReadWord(GlobalVariableTableAddressIndex);
        }

        public static ushort ReadStaticMemoryBase(this Memory memory)
        {
            return memory.ReadWord(StaticMemoryBaseIndex);
        }

        public static ushort ReadFlags2(this Memory memory)
        {
            return memory.ReadWord(Flags2Index);
        }

        public static string ReadSerialNumber(this Memory memory)
        {
            return memory.ReadAsciiString(SerialNumberIndex, 6);
        }

        public static ushort ReadAbbreviationsTableAddress(this Memory memory)
        {
            return memory.ReadWord(AbbreviationTableAddressIndex);
        }

        public static ushort ReadFileSize(this Memory memory)
        {
            return memory.ReadWord(FileSizeIndex);
        }

        public static ushort ReadChecksum(this Memory memory)
        {
            return memory.ReadWord(ChecksumIndex);
        }

        public static ushort ReadRoutinesOffset(this Memory memory)
        {
            return memory.ReadWord(RoutinesOffsetIndex);
        }

        public static ushort ReadStringsOffset(this Memory memory)
        {
            return memory.ReadWord(StringsOffsetIndex);
        }

        public static ushort ReadTerminatingCharactersTableAddress(this Memory memory)
        {
            return memory.ReadWord(TerminatingCharactersTableAddressIndex);
        }

        public static ushort ReadAlphabetTableAddress(this Memory memory)
        {
            return memory.ReadWord(AlphabetTableAddressIndex);
        }

        public static ushort ReadHeaderExtensionTableAddress(this Memory memory)
        {
            return memory.ReadWord(HeaderExtensionTableAddressIndex);
        }

        public static string ReadInformVersionNumber(this Memory memory)
        {
            return memory.ReadAsciiString(InformVersionNumberIndex, 4);
        }

        public static int UnpackRoutineAddress(this Memory memory, ushort byteAddress)
        {
            var version = memory.ReadVersion();
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
                    var routinesOffset = memory.ReadRoutinesOffset();
                    return (byteAddress * 4) + (routinesOffset * 8);
                case 8:
                    return byteAddress * 8;
                default:
                    throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int UnpackStringAddress(this Memory memory, ushort byteAddress)
        {
            var version = memory.ReadVersion();
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
                    var stringsOffset = memory.ReadStringsOffset();
                    return (byteAddress * 4) + (stringsOffset * 8);
                case 8:
                    return byteAddress * 8;
                default:
                    throw new InvalidOperationException("Invalid version number: " + version);
            }
        }
    }
}
