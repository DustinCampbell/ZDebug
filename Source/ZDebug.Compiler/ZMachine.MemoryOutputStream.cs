using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Execution;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    public sealed partial class ZMachine
    {
        private class MemoryOutputStream : IOutputStream
        {
            private readonly byte[] memory;
            private readonly int address;
            private ushort count;

            public MemoryOutputStream(byte[] memory, int address)
            {
                this.memory = memory;
                this.address = address;

                this.count = 0;
                memory.WriteWord(address, this.count);
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
