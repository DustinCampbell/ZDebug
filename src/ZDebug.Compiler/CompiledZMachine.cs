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

namespace ZDebug.Compiler;

public sealed partial class CompiledZMachine : ZMachine
{
    internal const int STACK_SIZE = 65536;

    private readonly IZMachineProfiler profiler;
    private readonly bool precompile;
    private readonly bool debugging;

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
        this.debugging = debugging;

        objectTableAddress = Memory.ReadWord(0x0a);
        propertyDefaultsTableSize = (byte)(Version < 4 ? 31 : 63);
        objectEntriesAddress = (ushort)(objectTableAddress + (propertyDefaultsTableSize * 2));
        objectEntrySize = (byte)(Version < 4 ? 9 : 14);
        objectParentOffset = (byte)(Version < 4 ? 4 : 6);
        objectSiblingOffset = (byte)(Version < 4 ? 5 : 8);
        objectChildOffset = (byte)(Version < 4 ? 6 : 10);
        objectPropertyTableAddressOffset = (byte)(Version < 4 ? 7 : 12);
        objectAttributeByteCount = (byte)(Version < 4 ? 4 : 6);
        objectAttributeCount = (byte)(Version < 4 ? 32 : 48);

        dictionaryAddress = Memory.ReadWord(0x08);

        packResolution = Version < 4 ? 2 : Version < 8 ? 4 : 8;
        routinesOffset = (Version >= 6 && Version <= 7) ? Memory.ReadWord(0x28) : 0;
        stringsOffset = (Version >= 6 && Version <= 7) ? Memory.ReadWord(0x2a) : 0;

        routineTable = new ZRoutineTable(story);
        addressToRoutineCallMap = new IntegerMap<ZRoutineCall>(8192);
        compilationResults = new IntegerMap<ZCompilerResult>(8192);

        localArrayPool = new Stack<ushort[]>();

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
        var mainAddress = Memory.ReadWord(0x06);
        if (Version != 6)
        {
            mainAddress--;
        }

        return mainAddress;
    }

    internal ushort[] GetLocalArray(ZRoutine routine)
    {
        var result = localArrayPool.Count > 0
            ? localArrayPool.Pop()
            : new ushort[15];

        if (Version < 5)
        {
            var localCount = routine.Locals.Length;
            for (var i = 0; i < localCount; i++)
            {
                var localValue = routine.Locals[i];
                if (localValue > 0)
                {
                    result[i] = localValue;
                }
            }
        }

        return result;
    }

    internal void ReleaseLocalArray(ushort[] locals)
    {
        Array.Clear(locals, 0, 15);
        localArrayPool.Push(locals);
    }

    internal bool Verify() => Story.ActualChecksum == Header.ReadChecksum(Memory);

    private ZRoutine GetRoutineByAddress(int address)
    {
        if (!routineTable.TryGetByAddress(address, out var routine))
        {
            routineTable.Add(address);
            routine = routineTable.GetByAddress(address);
        }

        return routine;
    }

    internal ZCompilerResult Compile(ZRoutine routine)
    {
        if (!compilationResults.TryGetValue(routine.Address, out var result))
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
        if (!addressToRoutineCallMap.TryGetValue(address, out var routineCall))
        {
            cacheMiss++;
            var routine = GetRoutineByAddress(address);
            routineCall = new ZRoutineCall(routine, machine: this);
            addressToRoutineCallMap.Add(address, routineCall);
        }

        if (precompile && !compiling)
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
        var zwords = ZText.ReadZWords(address);
        return ConvertZText(zwords);
    }

    internal int NextRandom(short range)
    {
        // range should be inclusive, so we need to subtract 1 since System.Random.Next makes it exclusive
        const ushort minValue = 1;
        var maxValue = Math.Max(minValue, (ushort)(range - 1));
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

    internal string ConvertZText(ushort[] zwords) => ZText.ZWordsAsString(zwords, ZTextFlags.All);

    internal void Read_Z3(ushort textBuffer, ushort parseBuffer)
    {
        inputReceived = false;

        Screen.ShowStatus();

        var maxChars = Memory.ReadByte(textBuffer);

        Screen.ReadCommand(maxChars, s =>
        {
            var text = s.ToLower();

            for (var i = 0; i < text.Length; i++)
            {
                Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
            }

            Memory.WriteByte(textBuffer + 1 + text.Length, 0);

            // TODO: Use ztext.TokenizeLine.

            var tokens = ZText.TokenizeCommand(text, dictionaryAddress);

            var maxWords = Memory.ReadByte(parseBuffer);
            var parsedWords = Math.Min(maxWords, (byte)tokens.Length);

            Memory.WriteByte(parseBuffer + 1, parsedWords);

            for (var i = 0; i < parsedWords; i++)
            {
                var token = tokens[i];

                var address = ZText.LookupWord(token.Text, dictionaryAddress);
                if (address > 0)
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                }
                else
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                }

                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
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

        var maxChars = Memory.ReadByte(textBuffer);

        Screen.ReadCommand(maxChars, s =>
        {
            var text = s.ToLower();

            for (var i = 0; i < text.Length; i++)
            {
                Memory.WriteByte(textBuffer + 1 + i, (byte)text[i]);
            }

            Memory.WriteByte(textBuffer + 1 + text.Length, 0);

            // TODO: Use ztext.TokenizeLine.

            var tokens = ZText.TokenizeCommand(text, dictionaryAddress);

            var maxWords = Memory.ReadByte(parseBuffer);
            var parsedWords = Math.Min(maxWords, (byte)tokens.Length);

            Memory.WriteByte(parseBuffer + 1, parsedWords);

            for (var i = 0; i < parsedWords; i++)
            {
                var token = tokens[i];

                var address = ZText.LookupWord(token.Text, dictionaryAddress);
                if (address > 0)
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), address);
                }
                else
                {
                    Memory.WriteWord(parseBuffer + 2 + (i * 4), 0);
                }

                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 2, (byte)token.Length);
                Memory.WriteByte(parseBuffer + 2 + (i * 4) + 3, (byte)(token.Start + 1));
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
            var pTextBuffer = pMemory + textBuffer;

            var maxChars = *pTextBuffer++;

            Screen.ReadCommand(maxChars, s =>
            {
                var text = s.ToLower();

                var existingTextCount = *pTextBuffer;
                *pTextBuffer++ = (byte)text.Length;
                pTextBuffer += existingTextCount;

                for (var i = 0; i < text.Length; i++)
                {
                    *pTextBuffer++ = (byte)text[i];
                }

                if (parseBuffer > 0)
                {
                    ZText.TokenizeLine(textBuffer, parseBuffer, dictionaryAddress, flag: false);
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

        Screen.ReadChar(ch =>
        {
            result = (ushort)ch;
            inputReceived = true;
        });

        while (!inputReceived)
        {
        }

        return result;
    }

    internal void Tokenize(ushort textBuffer, ushort parseBuffer, ushort dictionary, bool flag) => ZText.TokenizeLine(textBuffer, parseBuffer, dictionary, flag);

    internal void op_copy_table(ushort first, ushort second, ushort size)
    {
        if (second == 0) // zero out first table
        {
            for (var j = 0; j < size; j++)
            {
                Memory.WriteByte(first + j, 0);
            }
        }
        else if ((short)size < 0 || first > second) // copy forwards
        {
            var copySize = size;
            if ((short)copySize < 0)
            {
                copySize = (ushort)-(short)size;
            }

            for (var j = 0; j < copySize; j++)
            {
                var value = Memory.ReadByte(first + j);
                Memory.WriteByte(second + j, value);
            }
        }
        else // copy backwards
        {
            for (var j = size - 1; j >= 0; j--)
            {
                var value = Memory.ReadByte(first + j);
                Memory.WriteByte(second + j, value);
            }
        }
    }

    internal ushort op_scan_table(ushort x, ushort table, ushort len, ushort form)
    {
        var address = table;

        for (var j = 0; j < len; j++)
        {
            if ((form & 0x80) != 0)
            {
                var value = Memory.ReadWord(address);
                if (value == x)
                {
                    return address;
                }
            }
            else
            {
                var value = Memory[address];
                if (value == x)
                {
                    return address;
                }
            }

            address += (ushort)(form & 0x7f);
        }

        return 0;
    }

    internal void op_print_table(ushort address, ushort width, ushort height, ushort skip)
    {
        var left = Screen.GetCursorColumn();

        for (var i = 0; i < height; i++)
        {
            if (i != 0)
            {
                var y = Screen.GetCursorLine() + 1;
                Screen.SetCursor(y, left);
            }

            for (var j = 0; j < width; j++)
            {
                var ch = (char)Memory.ReadByte(address);
                address++;
                Screen.Print(ch);
            }

            address += skip;
        }
    }

    internal void op_insert_obj(ushort objNum, ushort destNum) => Story.ObjectTable.MoveObjectToDestinationByNumber(objNum, destNum);

    internal void op_remove_obj(ushort objNum) => Story.ObjectTable.RemoveObjectFromParentByNumber(objNum);

    internal void SelectScreenStream() => OutputStreams.SelectScreenStream();

    internal void DeselectScreenStream() => OutputStreams.DeselectScreenStream();

    internal void SelectTranscriptStream() => OutputStreams.SelectTranscriptStream();

    internal void DeselectTranscriptStream() => OutputStreams.DeselectTranscriptStream();

    internal void SelectMemoryStream(int address) => OutputStreams.SelectMemoryStream(address);

    internal void DeselectMemoryStream() => OutputStreams.DeselectMemoryStream();

    internal void PrintText(string text) => OutputStreams.Print(text);

    internal void PrintChar(char ch) => OutputStreams.Print(ch);

    internal void ShowStatus() => Screen.ShowStatus();

    internal void SetTextStyle(ZTextStyle style) => Screen.SetTextStyle(style);

    internal void SplitWindow(int lines) => Screen.Split(lines);

    internal void SetWindow(int window) => Screen.SetWindow(window);

    internal void ClearWindow(short window)
    {
        if (window < 0)
        {
            if (window == -1)
            {
                Screen.ClearAll(unsplit: true);
            }
            else if (window == -2)
            {
                Screen.ClearAll(unsplit: false);
            }
            else
            {
                throw new ZMachineException("Invalid window operand");
            }
        }
        else
        {
            Screen.Clear(window);
        }
    }

    internal void SetCursor(int line, int column) => Screen.SetCursor(line - 1, column - 1);

    internal void SetColors(ZColor foreground, ZColor background)
    {
        if (foreground > 0)
        {
            Screen.SetForegroundColor(foreground);
        }

        if (background > 0)
        {
            Screen.SetBackgroundColor(background);
        }
    }

    public int UnpackRoutineAddress(ushort byteAddress) => (byteAddress * packResolution) + (routinesOffset * 8);

    public int UnpackStringAddress(ushort byteAddress) => (byteAddress * packResolution) + (stringsOffset * 8);

    public void Run()
    {
        stopping = false;
        var routineCall = GetRoutineCall(GetMainRoutineAddress());

        var stack = new ushort[STACK_SIZE];
        var sp = -1;

        routineCall.Invoke0(Memory, stack, sp);
    }

    internal void Tick()
    {
        if (stopping)
        {
            stopping = false;
            throw new ZMachineInterruptedException();
        }
    }

    public void Stop() => stopping = true;

    public bool Profiling => profiler != null;

    public bool Precompile => precompile;

    public bool Debugging => debugging;

    public ushort ObjectTableAddress => objectTableAddress;

    public byte PropertyDefaultsTableSize => propertyDefaultsTableSize;

    public ushort ObjectEntriesAddress => objectEntriesAddress;

    public byte ObjectEntrySize => objectEntrySize;

    public byte ObjectParentOffset => objectParentOffset;

    public byte ObjectSiblingOffset => objectSiblingOffset;

    public byte ObjectChildOffset => objectChildOffset;

    public byte ObjectPropertyTableAddressOffset => objectPropertyTableAddressOffset;

    public byte ObjectAttributesByteCount => objectAttributeByteCount;

    public byte ObjectAttributeCount => objectAttributeCount;

    public int RoutinesOffset => routinesOffset;

    public int StringsOffset => stringsOffset;
}
