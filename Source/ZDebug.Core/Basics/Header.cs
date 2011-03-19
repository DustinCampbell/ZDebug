using System;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    internal static class Header
    {
        // offsets...
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

        private static string ReadAsciiString(byte[] memory, int index, int length)
        {
            var chars = memory.ReadBytes(index, length).Select(b => (char)b);
            return new string(chars);
        }

        public static byte ReadVersion(byte[] memory)
        {
            return memory.ReadByte(VersionIndex);
        }

        public static ushort ReadReleaseNumber(byte[] memory)
        {
            return memory.ReadWord(ReleaseNumberIndex);
        }

        public static ushort ReadHighMemoryBase(byte[] memory)
        {
            return memory.ReadWord(HighMemoryBaseIndex);
        }

        public static ushort ReadInitialPC(byte[] memory)
        {
            return memory.ReadWord(InitialPCIndex);
        }

        public static int ReadMainRoutineAddress(byte[] memory)
        {
            if (Header.ReadVersion(memory) == 6)
            {
                return Header.UnpackRoutineAddress(memory, Header.ReadInitialPC(memory));
            }
            else
            {
                return Header.ReadInitialPC(memory) - 1;
            }
        }

        public static ushort ReadDictionaryAddress(byte[] memory)
        {
            return memory.ReadWord(DictionaryAddressIndex);
        }

        public static ushort ReadObjectTableAddress(byte[] memory)
        {
            return memory.ReadWord(ObjectTableAddressIndex);
        }

        public static ushort ReadGlobalVariableTableAddress(byte[] memory)
        {
            return memory.ReadWord(GlobalVariableTableAddressIndex);
        }

        public static ushort ReadStaticMemoryBase(byte[] memory)
        {
            return memory.ReadWord(StaticMemoryBaseIndex);
        }

        public static int ReadSerialNumber(byte[] memory)
        {
            var bytes = memory.ReadBytes(SerialNumberIndex, 6);
            var zero = (byte)'0';

            return ((bytes[0] - zero) * 100000) +
                ((bytes[1] - zero) * 10000) +
                ((bytes[2] - zero) * 1000) +
                ((bytes[3] - zero) * 100) +
                ((bytes[4] - zero) * 10) +
                (bytes[5] - zero);
        }

        public static string ReadSerialNumberText(byte[] memory)
        {
            return Header.ReadAsciiString(memory, SerialNumberIndex, 6);
        }

        public static ushort ReadAbbreviationsTableAddress(byte[] memory)
        {
            return memory.ReadWord(AbbreviationTableAddressIndex);
        }

        public static int ReadFileSize(byte[] memory)
        {
            var fileSize = memory.ReadWord(FileSizeIndex);

            var version = Header.ReadVersion(memory);
            switch (version)
            {
                case 1:
                case 2:
                case 3:
                    return fileSize * 2;
                case 4:
                case 5:
                    return fileSize * 4;
                case 6:
                case 7:
                case 8:
                    return fileSize * 8;
                default:
                    throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static ushort ReadChecksum(byte[] memory)
        {
            return memory.ReadWord(ChecksumIndex);
        }

        public static ushort CalculateChecksum(byte[] memory)
        {
            var size = Math.Min(Header.ReadFileSize(memory), memory.Length);
            ushort result = 0;
            for (int i = 0x40; i < size; i++)
            {
                result += memory.ReadByte(i);
            }

            return result;
        }

        public static ushort ReadRoutinesOffset(byte[] memory)
        {
            return memory.ReadWord(RoutinesOffsetIndex);
        }

        public static ushort ReadStringsOffset(byte[] memory)
        {
            return memory.ReadWord(StringsOffsetIndex);
        }

        public static ushort ReadTerminatingCharactersTableAddress(byte[] memory)
        {
            return memory.ReadWord(TerminatingCharactersTableAddressIndex);
        }

        public static ushort ReadAlphabetTableAddress(byte[] memory)
        {
            return memory.ReadWord(AlphabetTableAddressIndex);
        }

        public static ushort ReadHeaderExtensionTableAddress(byte[] memory)
        {
            return memory.ReadWord(HeaderExtensionTableAddressIndex);
        }

        public static bool IsInformStory(byte[] memory)
        {
            var serialNumber = Header.ReadSerialNumber(memory);
            return serialNumber < 800000 || serialNumber >= 930000;
        }

        public static int ReadInformVersionNumber(byte[] memory)
        {
            var informVersion = 0;
            var b1 = memory.ReadByte(InformVersionNumberIndex);
            if (b1 == 0)
            {
                return informVersion;
            }

            var zero = (byte)'0';

            var b2 = memory.ReadByte(InformVersionNumberIndex + 2);
            var b3 = memory.ReadByte(InformVersionNumberIndex + 3);

            return ((b1 - zero) * 100) + ((b2 - zero) * 10) + (b3 - zero);
        }

        public static string ReadInformVersionText(byte[] memory)
        {
            return Header.ReadAsciiString(memory, InformVersionNumberIndex, 4);
        }

        public static void WriteScreenHeightInLines(byte[] memory, byte screenHeight)
        {
            memory.WriteByte(0x20, screenHeight);
        }

        public static void WriteScreenWidthInColumns(byte[] memory, byte screenWidth)
        {
            memory.WriteByte(0x21, screenWidth);
        }

        public static void WriteScreenHeightInUnits(byte[] memory, ushort screenHeight)
        {
            memory.WriteWord(0x24, screenHeight);
        }

        public static void WriteScreenWidthInUnits(byte[] memory, ushort screenWidth)
        {
            memory.WriteWord(0x22, screenWidth);
        }

        public static void WriteFontHeightInUnits(byte[] memory, byte fontHeight)
        {
            if (Header.ReadVersion(memory) == 6)
            {
                memory.WriteByte(0x26, fontHeight);
            }
            else
            {
                memory.WriteByte(0x27, fontHeight);
            }
        }

        public static void WriteFontWidthInUnits(byte[] memory, byte fontWidth)
        {
            if (Header.ReadVersion(memory) == 6)
            {
                memory.WriteByte(0x27, fontWidth);
            }
            else
            {
                memory.WriteByte(0x26, fontWidth);
            }
        }

        public static int UnpackRoutineAddress(byte[] memory, ushort byteAddress)
        {
            var version = Header.ReadVersion(memory);
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
                    var routinesOffset = Header.ReadRoutinesOffset(memory);
                    return (byteAddress * 4) + (routinesOffset * 8);
                case 8:
                    return byteAddress * 8;
                default:
                    throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        public static int UnpackStringAddress(byte[] memory, ushort byteAddress)
        {
            var version = Header.ReadVersion(memory);
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
                    var stringsOffset = Header.ReadStringsOffset(memory);
                    return (byteAddress * 4) + (stringsOffset * 8);
                case 8:
                    return byteAddress * 8;
                default:
                    throw new InvalidOperationException("Invalid version number: " + version);
            }
        }
    }
}
