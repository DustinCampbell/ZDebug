using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Extensions;

namespace ZDebug.Terp.Profiling;

public partial class ZMachineProfiler : IZMachineProfiler
{
    private readonly List<RoutineCompilationStatistics> allStatistics;
    private readonly Dictionary<int, Routine> routines;
    private readonly List<Call> calls;
    private readonly Stack<Call> callStack;
    private TimeSpan runningTime;

    private readonly Dictionary<int, Tuple<int, TimeSpan>> instructionTimings;
    private readonly Stopwatch instructionTimer;

    private int routinesExecuted;
    private int instructionsExecuted;

    private readonly HashSet<int> calculatedCalls;
    private int directCallCount;
    private int calculatedCallCount;

    public ZMachineProfiler()
    {
        allStatistics = [];
        routines = [];
        calls = [];
        callStack = new Stack<Call>();

        instructionTimings = [];
        instructionTimer = new Stopwatch();

        calculatedCalls = [];
    }

    void IZMachineProfiler.RoutineCompiled(RoutineCompilationStatistics statistics)
    {
        allStatistics.Add(statistics);

        var address = statistics.Routine.Address;
        if (!routines.ContainsKey(address))
        {
            routines.Add(address, new Routine(this, address, statistics));
        }
    }

    void IZMachineProfiler.Call(int address, bool calculated)
    {
        if (calculated)
        {
            calculatedCalls.Add(address);
            calculatedCallCount++;
        }
        else
        {
            directCallCount++;
        }
    }

    void IZMachineProfiler.EnterRoutine(int address)
    {
        routinesExecuted++;

        var routine = routines[address];

        var index = calls.Count;
        var parent = callStack.Count > 0
            ? callStack.Peek().Index
            : -1;

        var recursive = callStack.TopToBottom().Any(c => c.Routine.Address == address);

        var call = new Call(this, routine, index, parent, recursive);
        calls.Add(call);
        callStack.Push(call);

        call.Enter();
    }

    void IZMachineProfiler.ExitRoutine(int address)
    {
        var call = callStack.Pop();
        call.Exit();
    }

    void IZMachineProfiler.ExecutingInstruction(int address) => instructionTimer.Restart();

    void IZMachineProfiler.ExecutedInstruction(int address)
    {
        instructionTimer.Stop();

        if (instructionTimings.TryGetValue(address, out var timings))
        {
            timings = Tuple.Create(timings.Item1 + 1, timings.Item2.Add(instructionTimer.Elapsed));
        }
        else
        {
            timings = Tuple.Create(1, instructionTimer.Elapsed);
        }

        instructionTimings[address] = timings;

        instructionsExecuted++;
    }

    void IZMachineProfiler.Quit()
    {
    }

    void IZMachineProfiler.Interrupt()
    {
    }

    private Call GetCallByIndex(int index) => calls[index];

    public void Stop(TimeSpan runningTime)
    {
        this.runningTime = runningTime;
        while (callStack.Count > 0)
        {
            var call = callStack.Pop();
            call.Exit();
        }

        foreach (var routine in routines.Values)
        {
            routine.Done();
        }
    }

    public IEnumerable<RoutineCompilationStatistics> CompilationStatistics => allStatistics.ToList();

    public double GetAverageOpcodeILSize(string opcodeName)
    {
        long totalILSize = 0;
        long numberOpcodes = 0;
        foreach (var routineStat in allStatistics)
        {
            foreach (var stat in routineStat.InstructionStatistics)
            {
                if (stat.Instruction.Opcode.Name == opcodeName)
                {
                    numberOpcodes++;
                    totalILSize += stat.Size;
                }
            }
        }

        if (numberOpcodes == 0)
        {
            return 0;
        }

        return (double)totalILSize / (double)numberOpcodes;
    }

    public int RoutinesCompiled => allStatistics.Count;

    public int RoutinesExecuted => routinesExecuted;

    public int InstructionsExecuted => instructionsExecuted;

    public ICall RootCall => calls[0];

    public IEnumerable<IRoutine> Routines
    {
        get
        {
            foreach (var routine in routines.Values)
            {
                yield return routine;
            }
        }
    }

    public IEnumerable<Tuple<int, Tuple<int, TimeSpan>>> InstructionTimings
    {
        get
        {
            foreach (var timing in instructionTimings)
            {
                var address = timing.Key;
                var timings = timing.Value;
                yield return Tuple.Create(address, timings);
            }
        }
    }

    public TimeSpan RunningTime => runningTime;

    public int DirectCallCount => directCallCount;

    public int CalculatedCallCount => calculatedCallCount;
}
