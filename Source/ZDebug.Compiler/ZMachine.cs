using System;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Execution;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    public sealed partial class ZMachine
    {
        internal const int STACK_SIZE = 32768;

        private readonly byte[] memory;
        private readonly IScreen screen;
        private readonly IZMachineProfiler profiler;
        private readonly bool debugging;
        private readonly OutputStreams outputStreams;
        private readonly ZText ztext;

        // routine state
        private int currentRoutineAddress;

        private readonly ushort[] stack;
        private int sp;

        private int stackFrame;
        private readonly int[] stackFrames;
        private int sfp;

        private readonly ushort[] locals;
        private ushort localCount;

        private readonly ushort[] arguments;
        private ushort argumentCount;

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

        private readonly ushort dictionaryAddress;
        private readonly ushort globalVariableTableAddress;

        private readonly int packResolution;
        private readonly int routinesOffset;
        private readonly int stringsOffset;

        private readonly IntegerMap<ZCompilerResult> compiledRoutines;

        private Random random;

        private int currentAddress = -1;
        private volatile bool interrupt;
        private volatile bool inputReceived;

        public ZMachine(byte[] memory, IScreen screen = null, IZMachineProfiler profiler = null, bool debugging = false)
        {
            this.memory = memory;
            this.screen = screen;
            this.profiler = profiler;
            this.debugging = debugging;
            this.outputStreams = new OutputStreams(memory);
            this.outputStreams.RegisterScreen(screen);
            this.ztext = new ZText(new Memory(memory));
            this.version = memory.ReadByte(0x00);

            this.stack = new ushort[STACK_SIZE];
            this.sp = -1;

            this.stackFrame = -1;
            this.stackFrames = new int[STACK_SIZE];
            this.sfp = -1;

            this.locals = new ushort[15];
            this.localCount = 0;

            this.arguments = new ushort[7];
            this.argumentCount = 0;

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

            this.dictionaryAddress = memory.ReadWord(0x08);
            this.globalVariableTableAddress = memory.ReadWord(0x0c);

            this.packResolution = this.version < 4 ? 2 : this.version < 8 ? 4 : 8;
            this.routinesOffset = (this.version >= 6 && this.version <= 7) ? memory.ReadWord(0x28) : 0;
            this.stringsOffset = (this.version >= 6 && this.version <= 7) ? memory.ReadWord(0x2a) : 0;

            this.compiledRoutines = new IntegerMap<ZCompilerResult>(1024);

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

        internal void PushFrame(int address)
        {
            // argument count
            // local variable values (reversed)
            // local variable count

            stackFrames[++sfp] = stackFrame;

            var stack = this.stack;
            var sp = this.sp;

            stack[++sp] = this.argumentCount;

            var localCount = this.localCount;
            var locals = this.locals;
            for (int i = localCount - 1; i >= 0; i--)
            {
                stack[++sp] = locals[i];
            }

            stack[++sp] = (ushort)localCount;

            this.stackFrame = sp;
            this.sp = sp;
        }

        internal void PopFrame()
        {
            // local variable count
            // local variable values
            // argument count

            var stack = this.stack;
            var sp = this.sp;

            sp = this.stackFrame;

            var localCount = stack[sp--];
            var locals = this.locals;
            for (int i = 0; i < localCount; i++)
            {
                locals[i] = stack[sp--];
            }

            this.localCount = localCount;
            this.argumentCount = stack[sp--];

            this.stackFrame = stackFrames[sfp--];
            this.sp = sp;
        }

        private ZCompilerResult GetCompilerResult(int address)
        {
            ZCompilerResult result;
            if (!compiledRoutines.TryGetValue(address, out result))
            {
                result = ZCompiler.Compile(
                    routine: ZRoutine.Create(address, memory),
                    machine: this,
                    profiling: profiler != null);

                compiledRoutines.Add(address, result);

                if (profiler != null)
                {
                    profiler.RoutineCompiled(result.Statistics);
                }
            }

            return result;
        }

        private ZRoutineCode SetupCall(int address, ushort argCount)
        {
            var compilerResult = GetCompilerResult(address);

            PushFrame(address);

            this.argumentCount = argCount;

            var locals = compilerResult.Routine.Locals;
            var localCount = (ushort)locals.Length;

            for (int i = argCount; i < localCount; i++)
            {
                this.locals[i] = locals[i];
            }

            this.localCount = localCount;

            return compilerResult.Code;
        }

        internal ushort Call0(int address)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 0);

            return code();
        }

        internal ushort Call1(int address, ushort arg0)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 1);

            this.locals[0] = arg0;

            return code();
        }

        internal ushort Call2(int address, ushort arg0, ushort arg1)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 2);

            this.locals[0] = arg0;
            this.locals[1] = arg1;

            return code();
        }

        internal ushort Call3(int address, ushort arg0, ushort arg1, ushort arg2)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 3);

            this.locals[0] = arg0;
            this.locals[1] = arg1;
            this.locals[2] = arg2;

            return code();
        }

        internal ushort Call4(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 4);

            this.locals[0] = arg0;
            this.locals[1] = arg1;
            this.locals[2] = arg2;
            this.locals[3] = arg3;

            return code();
        }

        internal ushort Call5(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 5);

            this.locals[0] = arg0;
            this.locals[1] = arg1;
            this.locals[2] = arg2;
            this.locals[3] = arg3;
            this.locals[4] = arg4;

            return code();
        }

        internal ushort Call6(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 6);

            this.locals[0] = arg0;
            this.locals[1] = arg1;
            this.locals[2] = arg2;
            this.locals[3] = arg3;
            this.locals[4] = arg4;
            this.locals[5] = arg5;

            return code();
        }

        internal ushort Call7(int address, ushort arg0, ushort arg1, ushort arg2, ushort arg3, ushort arg4, ushort arg5, ushort arg6)
        {
            if (address == 0)
            {
                return 0;
            }

            var code = SetupCall(address, 7);

            this.locals[0] = arg0;
            this.locals[1] = arg1;
            this.locals[2] = arg2;
            this.locals[3] = arg3;
            this.locals[4] = arg4;
            this.locals[5] = arg5;
            this.locals[6] = arg6;

            return code();
        }

        internal ZRoutineCode GetRoutineCode(int address)
        {
            return GetCompilerResult(address).Code;
        }

        internal void EnterRoutine(int address)
        {
            if (profiler != null)
            {
                profiler.EnterRoutine(address);
            }
        }

        internal void ExitRoutine(int address)
        {
            if (profiler != null)
            {
                profiler.ExitRoutine(address);
            }
        }

        internal void ExecutingInstruction(int address)
        {
            if (profiler != null)
            {
                if (currentAddress >= 0)
                {
                    ExecutedInstruction();
                }

                currentAddress = address;
                profiler.ExecutingInstruction(address);
            }
        }

        internal void ExecutedInstruction()
        {
            if (profiler != null)
            {
                profiler.ExecutedInstruction(currentAddress);
            }
        }

        internal void Quit()
        {
            if (profiler != null)
            {
                profiler.Quit();
            }
        }

        internal void Interrupt()
        {
            if (profiler != null)
            {
                profiler.Interrupt();
            }
        }

        internal string ReadZText(int address)
        {
            var zwords = ztext.ReadZWords(address);
            return ConvertZText(zwords);
        }

        internal int NextRandom(short range)
        {
            // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
            const int minValue = 1;
            int maxValue = Math.Max(minValue, range - 1);
            var result = random.Next(minValue, maxValue);

            return result;
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

        public void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }

        internal string ConvertZText(ushort[] zwords)
        {
            return ztext.ZWordsAsString(zwords, ZTextFlags.All);
        }

        internal void Read_Z3(ushort textBuffer, ushort parseBuffer)
        {
            inputReceived = false;

            screen.ShowStatus();

            byte maxChars = memory.ReadByte(textBuffer);

            screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ZCommandToken[] tokens = ztext.TokenizeCommand(text, dictionaryAddress);

                byte maxWords = memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = ztext.LookupWord(token.Text, dictionaryAddress);
                    if (address > 0)
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }

                inputReceived = true;
            });

            while (!inputReceived)
            {
            }
        }

        internal void Read_Z4(ushort textBuffer, ushort parseBuffer)
        {
            // TODO: Support timed input

            inputReceived = false;

            byte maxChars = memory.ReadByte(textBuffer);

            screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ZCommandToken[] tokens = ztext.TokenizeCommand(text, dictionaryAddress);

                byte maxWords = memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = ztext.LookupWord(token.Text, dictionaryAddress);
                    if (address > 0)
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }

                inputReceived = true;
            });

            while (!inputReceived)
            {
            }
        }

        internal ushort Read_Z5(ushort textBuffer, ushort parseBuffer)
        {
            // TODO: Support timed input

            inputReceived = false;
            ushort result = 0;

            byte maxChars = memory.ReadByte(textBuffer);

            screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                byte existingTextCount = memory.ReadByte(textBuffer + 1);

                memory.WriteByte(textBuffer + existingTextCount + 1, (byte)text.Length);

                for (int i = 0; i < text.Length; i++)
                {
                    memory.WriteByte(textBuffer + existingTextCount + 2 + i, (byte)text[i]);
                }

                if (parseBuffer > 0)
                {
                    // TODO: Use ztext.TokenizeLine.

                    ZCommandToken[] tokens = ztext.TokenizeCommand(text, dictionaryAddress);

                    byte maxWords = memory.ReadByte(parseBuffer);
                    byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                    memory.WriteByte(parseBuffer + 1, parsedWords);

                    for (int i = 0; i < parsedWords; i++)
                    {
                        ZCommandToken token = tokens[i];

                        ushort address = ztext.LookupWord(token.Text, dictionaryAddress);
                        if (address > 0)
                        {
                            memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                        }
                        else
                        {
                            memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                        }

                        memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                        memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 2));
                    }
                }

                // TODO: Update this when timed input is supported
                result = 10;

                inputReceived = true;
            });

            while (!inputReceived)
            {
            }

            return result;
        }

        internal ushort ReadChar()
        {
            inputReceived = false;
            ushort result = 0;

            screen.ReadChar(ch =>
            {
                result = (ushort)ch;
                inputReceived = true;
            });

            while (!inputReceived)
            {
            }

            return result;
        }

        internal void Tokenize(ushort textBuffer, ushort parseBuffer, ushort dictionary, bool flag)
        {
            ztext.TokenizeLine(textBuffer, parseBuffer, dictionary, flag);
        }

        internal void Tick()
        {

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
            interrupt = false;
            var mainAddress = memory.ReadWord(0x06);
            if (version != 6)
            {
                mainAddress--;
            }

            var code = GetRoutineCode(mainAddress);
            code();
        }

        public void Stop()
        {
            interrupt = true;
            inputReceived = true;
        }

        public bool Debugging
        {
            get { return debugging; }
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
