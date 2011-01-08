using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core;
using ZDebug.Core.Basics;

namespace ZDebug.Compiler
{
    public sealed class ZMachine
    {
        private readonly byte[] memory;

        public ZMachine(byte[] memory)
        {
            this.memory = memory;
        }

        private byte ReadByte(int value)
        {
            return memory[value];
        }

        private byte ReadByte(ushort value)
        {
            return memory[value];
        }
    }
}
