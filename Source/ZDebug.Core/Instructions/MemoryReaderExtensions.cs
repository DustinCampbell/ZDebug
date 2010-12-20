using System.Collections.Generic;
using ZDebug.Core.Basics;

namespace ZDebug.Core.Instructions
{
    /// <summary>
    /// MemoryReader extension methods specific to instructions.
    /// </summary>
    internal static class MemoryReaderExtensions
    {
        public static Variable NextVariable(this MemoryReader reader)
        {
            return Variable.FromByte(reader.NextByte());
        }

        public static Branch NextBranch(this MemoryReader reader)
        {
            var b1 = reader.NextByte();

            var condition = (b1 & 0x80) == 0x80;

            short offset;
            if ((b1 & 0x40) == 0x40) // is single byte
            {
                // bottom 6 bits
                offset = (short)(b1 & 0x3f);
            }
            else // is two bytes
            {
                // OR bottom 6 bits with the next byte
                b1 = (byte)(b1 & 0x3f);
                var b2 = reader.NextByte();
                var tmp = (ushort)((b1 << 8) | b2);

                // if bit 13, set bits 14 and 15 as well to produce proper signed value.
                if ((tmp & 0x2000) == 0x2000)
                {
                    tmp = (ushort)(tmp | 0xc000);
                }

                offset = (short)tmp;
            }

            return new Branch(condition, offset);
        }

        public static ushort[] NextZWords(this MemoryReader reader)
        {
            var list = new List<ushort>();

            while (true)
            {
                var zword = reader.NextWord();
                list.Add(zword);

                if ((zword & 0x8000) == 0x8000)
                {
                    break;
                }
            }

            return list.ToArray();
        }
    }
}
