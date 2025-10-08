using System;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Routines;

namespace ZDebug.Compiler;

internal sealed class ZCompilerResult
{
    public readonly ZRoutine Routine;
    public readonly RoutineCompilationStatistics Statistics;

    internal readonly ZRoutineCode Code;
    internal readonly ZRoutineCall[] Calls;

    internal ZCompilerResult(ZRoutine routine, ZRoutineCall[] calls, ZRoutineCode code, RoutineCompilationStatistics statistics)
    {
        ArgumentNullException.ThrowIfNull(routine);
        ArgumentNullException.ThrowIfNull(calls);
        ArgumentNullException.ThrowIfNull(code);
        ArgumentNullException.ThrowIfNull(statistics);

        this.Routine = routine;
        this.Calls = calls;
        this.Code = code;
        this.Statistics = statistics;
    }
}
