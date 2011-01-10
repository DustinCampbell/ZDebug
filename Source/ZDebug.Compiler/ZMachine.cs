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

        private readonly byte version;
        private readonly int objectTableAddress;
        private readonly int globalVariableTableAddress;

        private readonly int packResolution;
        private readonly int routinesOffset;
        private readonly int stringsOffset;

        private readonly IntegerMap<ZRoutineCode> compiledRoutines;

        public ZMachine(byte[] memory)
        {
            this.memory = memory;
            this.version = memory.ReadByte(0x00);
            this.objectTableAddress = memory.ReadWord(0x0a);
            this.globalVariableTableAddress = memory.ReadWord(0x0c);

            this.packResolution = this.version < 4 ? 2 : this.version < 8 ? 4 : 8;
            this.routinesOffset = (this.version >= 6 && this.version <= 7) ? memory.ReadWord(0x28) : 0;
            this.stringsOffset = (this.version >= 6 && this.version <= 7) ? memory.ReadWord(0x2a) : 0;

            this.compiledRoutines = new IntegerMap<ZRoutineCode>(1024);
        }

        private ZRoutineCode GetRoutineCode(int address)
        {
            ZRoutineCode result;
            if (!compiledRoutines.TryGetValue(address, out result))
            {
                var routine = ZRoutine.Create(address, memory);
                result = ZCompiler.Compile(routine, this);
                compiledRoutines.Add(address, result);
            }

            return result;
        }

        internal ushort Call(int address, ushort[] args)
        {
            var code = GetRoutineCode(address);
            return code(args);
        }

        public int UnpackRoutineAddress(ushort byteAddress)
        {
            return (byteAddress * packResolution) + (routinesOffset * 8);
        }

        public int UnpackStringAddress(ushort byteAddress)
        {
            return (byteAddress * packResolution) + (stringsOffset * 8);
        }

        public byte Version
        {
            get { return version; }
        }

        public int ObjectTableAddress
        {
            get { return objectTableAddress; }
        }

        public int GlobalVariableTableAddress
        {
            get { return globalVariableTableAddress; }
        }

        public int RoutinesOffset
        {
            get { return routinesOffset; }
        }

        public int StringsOffset
        {
            get { return stringsOffset; }
        }
    }
}
