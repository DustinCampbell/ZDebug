using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core;
using ZDebug.Core.Instructions;
using System.Collections.ObjectModel;

namespace ZDebug.Compiler
{
    public sealed class ZRoutine
    {
        private readonly int address;
        private readonly ReadOnlyCollection<Instruction> instructions;
        private readonly Action code;

        public ZRoutine(int address, Story story)
        {
            this.address = address;
        }
    }
}
