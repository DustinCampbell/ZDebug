﻿using ZDebug.Core.Basics;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private class MemoryOutputStream : IOutputStream
        {
            private readonly Memory memory;
            private readonly int address;
            private ushort count;

            public MemoryOutputStream(Memory memory, int address)
            {
                this.memory = memory;
                this.address = address;
            }

            private byte CharToByte(char ch)
            {
                return ch == '\n' ? (byte)13 : (byte)ch;
            }

            public void Print(string text)
            {
                var bytes = text.ToCharArray().ConvertAll(CharToByte);
                memory.WriteBytes(address + 2 + count, bytes);
                count += (ushort)bytes.Length;
                memory.WriteWord(address, count);
            }

            public void Print(char ch)
            {
                memory.WriteByte(address + 2 + count, CharToByte(ch));
                count++;
                memory.WriteWord(address, count);
            }
        }
    }
}
