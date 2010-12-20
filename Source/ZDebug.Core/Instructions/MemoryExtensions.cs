using ZDebug.Core.Basics;

namespace ZDebug.Core.Instructions
{
    /// <summary>
    /// MemoryReader extension methods specific to instructions.
    /// </summary>
    internal static class MemoryExtensions
    {
        public static Variable ReadVariable(this Memory memory, ref int address)
        {
            return Variable.FromByte(memory.ReadByte(ref address));
        }

        public static Branch ReadBranch(this Memory memory, ref int address)
        {
            var b1 = memory.ReadByte(ref address);

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
                var b2 = memory.ReadByte(ref address);
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

        public static ushort[] ReadZWords(this Memory memory, ref int address)
        {
            int count = 0;
            while (true)
            {
                var zword = memory.ReadWord(address + (count++ * 2));
                if ((zword & 0x8000) != 0)
                {
                    break;
                }
            }

            return memory.ReadWords(ref address, count);
        }
    }
}
