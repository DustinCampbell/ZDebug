using System;
using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Text
{
    internal sealed class AlphabetTable
    {
        private static readonly char[] A0 = "      abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] A1 = "      ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] A2 = "       \n0123456789.,!?_#'\"/\\-:()".ToCharArray();
        private static readonly char[] A3 = "       0123456789.,!?_#'\"/\\<-:()".ToCharArray();

        private readonly char[][] alphabets;

        private int currentAlphabet;
        private int baseAlphabet;

        public AlphabetTable(byte[] memory)
        {
            var version = Header.ReadVersion(memory);
            if (version == 1)
            {
                alphabets = new char[][] { A0, A1, A3 };
            }
            else if (version >= 2 && version <= 4)
            {
                alphabets = new char[][] { A0, A1, A2 };
            }
            else if (version >= 5 && version <= 8)
            {
                var alphabetTableAddress = Header.ReadAlphabetTableAddress(memory);
                if (alphabetTableAddress == 0)
                {
                    alphabets = new char[][] { A0, A1, A2 };
                }
                else
                {
                    alphabets = ReadCustomAlphabetTable(memory, alphabetTableAddress);
                }
            }
            else
            {
                throw new InvalidOperationException("Invalid version number: " + version);
            }
        }

        private static char ByteToChar(byte b)
        {
            var ch = (char)b;
            return ch == '^'
                ? '\n'
                : ch;
        }

        private static char[][] ReadCustomAlphabetTable(byte[] memory, int address)
        {
            var result = new char[3][];

            result[0] = "??????".ToCharArray().Concat(memory.ReadBytes(address, 26).ConvertAll(ByteToChar));
            result[1] = "??????".ToCharArray().Concat(memory.ReadBytes(address + 26, 26).ConvertAll(ByteToChar));

            // We need the first character below because A2/C6 isn't a printable character --
            // it starts a multi-byte ZSCII character.
            result[2] = "???????".ToCharArray().Concat(memory.ReadBytes(address + 53, 25).ConvertAll(ByteToChar));

            return result;
        }

        public void FullReset()
        {
            baseAlphabet = 0;
            currentAlphabet = 0;
        }

        public void Reset()
        {
            currentAlphabet = baseAlphabet;
        }

        public void Shift()
        {
            currentAlphabet = (baseAlphabet + 1) % 3;
        }

        public void DoubleShift()
        {
            currentAlphabet = (baseAlphabet + 2) % 3;
        }

        public void ShiftLock()
        {
            baseAlphabet = (baseAlphabet + 1) % 3;
            currentAlphabet = baseAlphabet;
        }

        public void DoubleShiftLock()
        {
            baseAlphabet = (baseAlphabet + 2) % 3;
            currentAlphabet = baseAlphabet;
        }

        public char ReadChar(byte zchar)
        {
            if (zchar < 6 || zchar > 31)
            {
                throw new ArgumentOutOfRangeException("zchar");
            }

            var result = alphabets[currentAlphabet][zchar];
            Reset();

            return result;
        }

        public int CurrentAlphabet
        {
            get { return currentAlphabet; }
        }

        public Tuple<byte, byte> FindSetAndIndexOfChar(char ch)
        {
            for (byte set = 0; set < 3; set++)
            {
                for (byte index = 6; index < 32; index++)
                {
                    if (alphabets[set][index] == ch)
                    {
                        return Tuple.Create(set, index);
                    }
                }
            }

            return null;
        }
    }
}
