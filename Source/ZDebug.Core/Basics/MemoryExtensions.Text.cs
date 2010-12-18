using System;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Basics
{
    internal static partial class MemoryExtensions
    {
        public static string[] ReadCustomAlphabetTable(this Memory memory, int address)
        {
            var result = new string[3];

            Converter<byte, char> byteToChar = b =>
            {
                var ch = (char)b;
                if (ch == '^')
                {
                    return '\n';
                }
                else
                {
                    return ch;
                }
            };

            var reader = memory.CreateReader(address);
            result[0] = "??????" + reader.NextBytes(26).ConvertAll(byteToChar).AsString();
            result[1] = "??????" + reader.NextBytes(26).ConvertAll(byteToChar).AsString();

            reader.Skip(1);
            result[2] = "???????" + reader.NextBytes(25).ConvertAll(byteToChar).AsString();

            return result;
        }

        public static ushort[] ReadAbbreviation(this Memory memory, int index)
        {
            if (index < 0 || index > 95)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            var abbreviationsTableAddress = memory.ReadAbbreviationsTableAddress();
            var abbreviationAddress = (2 * memory.ReadWord(abbreviationsTableAddress + (index * 2)));
            var reader = memory.CreateReader(abbreviationAddress);
            return reader.NextZWords();
        }
    }
}
