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
    public sealed partial class ZMachine
    {
        private readonly byte[] memory;
        private readonly IScreen screen;
        private readonly OutputStreams outputStreams;
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

        private Random random;

        public ZMachine(byte[] memory, IScreen screen = null)
        {
            this.memory = memory;
            this.screen = screen;
            this.outputStreams = new OutputStreams(memory);
            this.outputStreams.RegisterScreen(screen);
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

            this.random = new Random();

            if (version >= 4)
            {
                memory.WriteByte(0x20, screen.ScreenHeightInLines);
                memory.WriteByte(0x21, screen.ScreenWidthInColumns);
            }

            if (version >= 5)
            {
                memory.WriteWord(0x24, screen.ScreenHeightInUnits);
                memory.WriteWord(0x22, screen.ScreenWidthInUnits);

                if (version == 6)
                {
                    memory.WriteByte(0x26, screen.FontHeightInUnits);
                }
                else
                {
                    memory.WriteByte(0x27, screen.FontHeightInUnits);
                }

                if (version == 6)
                {
                    memory.WriteByte(0x27, screen.FontWidthInUnits);
                }
                else
                {
                    memory.WriteByte(0x26, screen.FontWidthInUnits);
                }
            }

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

        internal int NextRandom(short range)
        {
            // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
            const int minValue = 1;
            int maxValue = Math.Max(minValue, range - 1);
            return random.Next(minValue, maxValue);
        }

        internal void SeedRandom(short range)
        {
            if (range == 0)
            {
                random = new Random((int)DateTime.Now.Ticks);
            }
            else
            {
                random = new Random(+range);
            }
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
