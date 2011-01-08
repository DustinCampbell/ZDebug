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

        private ushort[] Mock(ushort[] values)
        {
            var result = new ushort[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                result[i] = values[i];
            }

            return result;
        }
    }
}
