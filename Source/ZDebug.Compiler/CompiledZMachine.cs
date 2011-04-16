using System;
using System.Collections.Generic;
using ZDebug.Compiler.Profiling;
using ZDebug.Core;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Execution;
using ZDebug.Core.Extensions;
using ZDebug.Core.Routines;
using ZDebug.Core.Text;

namespace ZDebug.Compiler
{
    public sealed partial class CompiledZMachine : ZMachine
    {
        internal const int STACK_SIZE = 32768;

        private readonly IZMachineProfiler profiler;
        private readonly bool precompile;
        private readonly bool debugging;

        // routine state
        private readonly ushort[] stack;
        private int sp;

        private int stackFrame;
        private readonly int[] stackFrames;
        private int sfp;

        private readonly ushort[] locals;
        private ushort localCount;

        private readonly ushort[] arguments;
        private ushort argumentCount;

        private int cacheMiss;

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

        private readonly int packResolution;
        private readonly int routinesOffset;
        private readonly int stringsOffset;

        private readonly ZRoutineTable routineTable;
        private readonly IntegerMap<ZRoutineCall> addressToRoutineCallMap;
        private readonly IntegerMap<ZCompilerResult> compilationResults;

        private readonly Stack<ushort[]> localArrayPool;

        private int currentAddress = -1;
        private volatile bool inputReceived;
        private volatile bool stopping;

        public CompiledZMachine(Story story, bool precompile = false, bool debugging = false, IZMachineProfiler profiler = null)
            : base(story)
        {
            this.profiler = profiler;
            this.precompile = precompile;

            this.stack = new ushort[STACK_SIZE];
            this.sp = -1;

            this.stackFrame = -1;
            this.stackFrames = new int[STACK_SIZE];
            this.sfp = -1;

            this.locals = new ushort[15];
            this.localCount = 0;

            this.arguments = new ushort[7];
            this.argumentCount = 0;

            this.objectTableAddress = this.Memory.ReadWord(0x0a);
            this.propertyDefaultsTableSize = (byte)(this.Version < 4 ? 31 : 63);
            this.objectEntriesAddress = (ushort)(this.objectTableAddress + (this.propertyDefaultsTableSize * 2));
            this.objectEntrySize = (byte)(this.Version < 4 ? 9 : 14);
            this.objectParentOffset = (byte)(this.Version < 4 ? 4 : 6);
            this.objectSiblingOffset = (byte)(this.Version < 4 ? 5 : 8);
            this.objectChildOffset = (byte)(this.Version < 4 ? 6 : 10);
            this.objectPropertyTableAddressOffset = (byte)(this.Version < 4 ? 7 : 12);
            this.objectAttributeByteCount = (byte)(this.Version < 4 ? 4 : 6);
            this.objectAttributeCount = (byte)(this.Version < 4 ? 32 : 48);

            this.dictionaryAddress = this.Memory.ReadWord(0x08);

            this.packResolution = this.Version < 4 ? 2 : this.Version < 8 ? 4 : 8;
            this.routinesOffset = (this.Version >= 6 && this.Version <= 7) ? Memory.ReadWord(0x28) : 0;
            this.stringsOffset = (this.Version >= 6 && this.Version <= 7) ? Memory.ReadWord(0x2a) : 0;

            this.routineTable = new ZRoutineTable(story);
            this.addressToRoutineCallMap = new IntegerMap<ZRoutineCall>(8192);
            this.compilationResults = new IntegerMap<ZCompilerResult>(8192);

            this.localArrayPool = new Stack<ushort[]>();

            if (this.precompile)
            {
                foreach (var routine in routineTable)
                {
                    GetRoutineCall(routine.Address);
                }

                this.precompile = false;
            }
        }

        private int GetMainRoutineAddress()
        {
            var mainAddress = this.Memory.ReadWord(0x06);
            if (this.Version != 6)
            {
                mainAddress--;
            }

            return mainAddress;
        }

        internal void PushFrame()
        {
            this.stackFrames[++this.sfp] = this.stackFrame;
            this.stackFrame = this.sp;
        }

        internal void PopFrame()
        {
            this.sp = this.stackFrame;
            this.stackFrame = this.stackFrames[this.sfp--];
        }

        internal ushort[] GetLocalArray(ZRoutine routine)
        {
            var result = localArrayPool.Count > 0
                ? localArrayPool.Pop()
                : new ushort[15];

            if (Version < 5)
            {
                var localCount = routine.Locals.Length;
                for (int i = 0; i < localCount; i++)
                {
                    result[i] = routine.Locals[i];
                }
            }

            return result;
        }

        internal void ReleaseLocalArray(ushort[] locals)
        {
            Array.Clear(locals, 0, 15);
            localArrayPool.Push(locals);
        }

        internal bool Verify()
        {
            return this.Story.ActualChecksum == Header.ReadChecksum(this.Memory);
        }

        private ZRoutine GetRoutineByAddress(int address)
        {
            ZRoutine routine;
            if (!routineTable.TryGetByAddress(address, out routine))
            {
                routineTable.Add(address);
                routine = routineTable.GetByAddress(address);
            }

            return routine;
        }

        internal ZCompilerResult Compile(ZRoutine routine)
        {
            ZCompilerResult result;
            if (!compilationResults.TryGetValue(routine.Address, out result))
            {
                result = ZCompiler.Compile(routine, machine: this);

                compilationResults.Add(routine.Address, result);

                if (profiler != null)
                {
                    profiler.RoutineCompiled(result.Statistics);
                }
            }

            return result;
        }

        private bool compiling;

        internal ZRoutineCall GetRoutineCall(int address)
        {
            ZRoutineCall routineCall;
            if (!addressToRoutineCallMap.TryGetValue(address, out routineCall))
            {
                cacheMiss++;
                var routine = GetRoutineByAddress(address);
                routineCall = new ZRoutineCall(routine, machine: this);
                addressToRoutineCallMap.Add(address, routineCall);
            }

            if (this.precompile && !compiling)
            {
                compiling = true;
                routineCall.Compile();
                compiling = false;
            }

            return routineCall;
        }

        internal ZRoutineCode GetRoutineCode(int address)
        {
            var routine = GetRoutineByAddress(address);
            return Compile(routine).Code;
        }

        internal void Profiler_Call(int address, bool calculated)
        {
            if (profiler != null)
            {
                profiler.Call(address, calculated);
            }
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
            var zwords = this.ZText.ReadZWords(address);
            return ConvertZText(zwords);
        }

        internal int NextRandom(short range)
        {
            // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
            const ushort minValue = 1;
            ushort maxValue = Math.Max(minValue, (ushort)(range - 1));
            var result = GenerateRandomNumber(minValue, maxValue);

            return result;
        }

        internal void SeedRandom(short range)
        {
            if (range == 0)
            {
                SetRandomSeed((int)DateTime.Now.Ticks);
            }
            else
            {
                SetRandomSeed(+range);
            }
        }

        internal string ConvertZText(ushort[] zwords)
        {
            return this.ZText.ZWordsAsString(zwords, ZTextFlags.All);
        }

        internal void Read_Z3(ushort textBuffer, ushort parseBuffer)
        {
            inputReceived = false;

            this.Screen.ShowStatus();

            byte maxChars = this.Memory.ReadByte(textBuffer);

            this.Screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    this.Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                this.Memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ZCommandToken[] tokens = this.ZText.TokenizeCommand(text, dictionaryAddress);

                byte maxWords = this.Memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                this.Memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = this.ZText.LookupWord(token.Text, dictionaryAddress);
                    if (address > 0)
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }

                inputReceived = true;
            });

            while (!inputReceived && !stopping)
            {
            }

            if (stopping)
            {
                throw new ZMachineInterruptedException();
            }
        }

        internal void Read_Z4(ushort textBuffer, ushort parseBuffer)
        {
            // TODO: Support timed input

            inputReceived = false;

            byte maxChars = this.Memory.ReadByte(textBuffer);

            this.Screen.ReadCommand(maxChars, s =>
            {
                string text = s.ToLower();

                for (int i = 0; i < text.Length; i++)
                {
                    this.Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
                }

                this.Memory.WriteByte(textBuffer + 1 + text.Length, 0);

                // TODO: Use ztext.TokenizeLine.

                ZCommandToken[] tokens = this.ZText.TokenizeCommand(text, dictionaryAddress);

                byte maxWords = this.Memory.ReadByte(parseBuffer);
                byte parsedWords = Math.Min(maxWords, (byte)tokens.Length);

                this.Memory.WriteByte(parseBuffer + 1, parsedWords);

                for (int i = 0; i < parsedWords; i++)
                {
                    ZCommandToken token = tokens[i];

                    ushort address = this.ZText.LookupWord(token.Text, dictionaryAddress);
                    if (address > 0)
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                    }
                    else
                    {
                        this.Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                    }

                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                    this.Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
                }

                inputReceived = true;
            });

            while (!inputReceived && !stopping)
            {
            }

            if (stopping)
            {
                throw new ZMachineInterruptedException();
            }
        }

        internal unsafe ushort Read_Z5(ushort textBuffer, ushort parseBuffer)
        {
            // TODO: Support timed input

            inputReceived = false;
            ushort result = 0;

            fixed (byte* pMemory = Memory)
            {
                byte* pTextBuffer = pMemory + textBuffer;

                byte maxChars = *pTextBuffer++;

                this.Screen.ReadCommand(maxChars, s =>
                {
                    string text = s.ToLower();

                    byte existingTextCount = *pTextBuffer;
                    *pTextBuffer++ = (byte)text.Length;
                    pTextBuffer += existingTextCount;

                    for (int i = 0; i < text.Length; i++)
                    {
                        *pTextBuffer++ = (byte)text[i];
                    }

                    if (parseBuffer > 0)
                    {
                        this.ZText.TokenizeLine(textBuffer, parseBuffer, dictionaryAddress, flag: false);
                    }

                    // TODO: Update this when timed input is supported
                    result = 10;

                    inputReceived = true;
                });
            }

            while (!inputReceived && !stopping)
            {
            }

            if (stopping)
            {
                throw new ZMachineInterruptedException();
            }

            return result;
        }

        internal ushort ReadChar()
        {
            inputReceived = false;
            ushort result = 0;

            this.Screen.ReadChar(ch =>
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
            this.ZText.TokenizeLine(textBuffer, parseBuffer, dictionary, flag);
        }

        internal void op_copy_table(ushort first, ushort second, ushort size)
        {
            if (second == 0) // zero out first table
            {
                for (int j = 0; j < size; j++)
                {
                    this.Memory.WriteByte(first + j, 0);
                }
            }
            else if ((short)size < 0 || first > second) // copy forwards
            {
                var copySize = size;
                if ((short)copySize < 0)
                {
                    copySize = (ushort)(-((short)size));
                }

                for (int j = 0; j < copySize; j++)
                {
                    var value = this.Memory.ReadByte(first + j);
                    this.Memory.WriteByte(second + j, value);
                }
            }
            else // copy backwards
            {
                for (int j = size - 1; j >= 0; j--)
                {
                    var value = this.Memory.ReadByte(first + j);
                    this.Memory.WriteByte(second + j, value);
                }
            }
        }

        internal ushort op_scan_table(ushort x, ushort table, ushort len, ushort form)
        {
            ushort address = table;

            for (int j = 0; j < len; j++)
            {
                if ((form & 0x80) != 0)
                {
                    var value = this.Memory.ReadWord(address);
                    if (value == x)
                    {
                        return address;
                    }
                }
                else
                {
                    var value = this.Memory[address];
                    if (value == x)
                    {
                        return address;
                    }
                }

                address += (ushort)(form & 0x7f);
            }

            return 0;
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
            stopping = false;
            var routineCall = GetRoutineCall(GetMainRoutineAddress());

            routineCall.Invoke0();
        }

        internal void Tick()
        {
            if (stopping)
            {
                stopping = false;
                throw new ZMachineInterruptedException();
            }
        }

        public void Stop()
        {
            stopping = true;
        }

        public bool Profiling
        {
            get { return profiler != null; }
        }

        public bool Precompile
        {
            get
            {
                return precompile;
            }
        }

        public bool Debugging
        {
            get
            {
                return debugging;
            }
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
