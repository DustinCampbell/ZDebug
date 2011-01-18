using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core;
using ZDebug.Core.Basics;
using ZDebug.Core.Utilities;
using ZDebug.Core.Collections;
using ZDebug.Core.Execution;
using ZDebug.Core.Text;

namespace ZDebug.Compiler
{
    public sealed class ZMachine
    {
        private readonly byte[] memory;
        private readonly IScreen screen;
        private readonly ZText ztext;

        private readonly byte version;

        private readonly ushort objectTableAddress;
        private readonly byte propertyDefaultsTableSize;
        private readonly ushort objectEntriesAddress;
        private readonly byte objectEntrySize;
        private readonly byte objectParentOffset;
        private readonly byte objectSiblingOffset;
        private readonly byte objectChildOffset;
        private readonly byte objectPropertyTableAddressOffset;
        private readonly byte objectAttributeByteCount;
        private readonly byte objectAttributeCount;

        private readonly ushort globalVariableTableAddress;

        private readonly int packResolution;
        private readonly int routinesOffset;
        private readonly int stringsOffset;

        private readonly IntegerMap<ZRoutineCode> compiledRoutines;

        public ZMachine(byte[] memory, IScreen screen = null)
        {
            this.memory = memory;
            this.screen = screen;
            this.ztext = new ZText(new Memory(memory));
            this.version = memory.ReadByte(0x00);

            this.objectTableAddress = memory.ReadWord(0x0a);
            this.propertyDefaultsTableSize = (byte)(this.version < 4 ? 31 : 63);
            this.objectEntriesAddress = (ushort)(this.objectTableAddress + (this.propertyDefaultsTableSize * 2));
            this.objectEntrySize = (byte)(this.version < 4 ? 9 : 14);
            this.objectParentOffset = (byte)(this.version < 4 ? 4 : 6);
            this.objectSiblingOffset = (byte)(this.version < 4 ? 5 : 8);
            this.objectChildOffset = (byte)(this.version < 4 ? 6 : 10);
            this.objectPropertyTableAddressOffset = (byte)(this.version < 4 ? 7 : 12);
            this.objectAttributeByteCount = (byte)(version < 4 ? 4 : 6);
            this.objectAttributeCount = (byte)(version < 4 ? 32 : 48);

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

        internal string ReadZText(int address)
        {
            var zwords = ztext.ReadZWords(address);
            return ConvertEmbeddedZText(zwords);
        }

        public string ConvertEmbeddedZText(ushort[] zwords)
        {
            return ztext.ZWordsAsString(zwords, ZTextFlags.All);
        }

        public int UnpackRoutineAddress(ushort byteAddress)
        {
            return (byteAddress * packResolution) + (routinesOffset * 8);
        }

        public int UnpackStringAddress(ushort byteAddress)
        {
            return (byteAddress * packResolution) + (stringsOffset * 8);
        }

        public void Run()
        {
            var mainAddress = memory.ReadWord(0x06);
            if (version != 6)
            {
                mainAddress--;
            }

            Call(mainAddress, new ushort[0]);
        }

        public byte Version
        {
            get { return version; }
        }

        public ushort ObjectTableAddress
        {
            get { return objectTableAddress; }
        }

        public byte PropertyDefaultsTableSize
        {
            get { return propertyDefaultsTableSize; }
        }

        public ushort ObjectEntriesAddress
        {
            get { return objectEntriesAddress; }
        }

        public byte ObjectEntrySize
        {
            get { return objectEntrySize; }
        }

        public byte ObjectParentOffset
        {
            get { return objectParentOffset; }
        }

        public byte ObjectSiblingOffset
        {
            get { return objectSiblingOffset; }
        }

        public byte ObjectChildOffset
        {
            get { return objectChildOffset; }
        }

        public byte ObjectPropertyTableAddressOffset
        {
            get { return objectPropertyTableAddressOffset; }
        }

        public byte ObjectAttributesByteCount
        {
            get { return objectAttributeByteCount; }
        }

        public byte ObjectAttributeCount
        {
            get { return objectAttributeCount; }
        }

        public ushort GlobalVariableTableAddress
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
