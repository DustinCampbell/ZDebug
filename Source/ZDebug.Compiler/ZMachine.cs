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

        private ushort Init()
        {
            bool b = true;
            if (b)
            {
                var stack = new ushort[1024];
                var sp = 0;

                return stack[sp++];
            }
            else
                return 0;
        }
    }
}
