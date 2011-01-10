using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core;
using ZDebug.Core.Basics;
using ZDebug.Core.Utilities;
using ZDebug.Core.Collections;

namespace ZDebug.Compiler
{
    public sealed class ZMachine
    {
        private readonly byte[] memory;
        private readonly int objectTableAddress;
        private readonly int globalVariableTableAddress;

        private readonly IntegerMap<ZRoutineCode> compiledRoutines;

        public ZMachine(byte[] memory)
        {
            this.memory = memory;
            this.objectTableAddress = memory.ReadWord(0x0a);
            this.globalVariableTableAddress = memory.ReadWord(0x0c);
        }

        public int ObjectTableAddress
        {
            get { return objectTableAddress; }
        }

        public int GlobalVariableTableAddress
        {
            get { return globalVariableTableAddress; }
        }
    }
}
