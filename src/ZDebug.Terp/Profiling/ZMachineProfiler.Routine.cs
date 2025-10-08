using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZDebug.Compiler.Profiling;

namespace ZDebug.Terp.Profiling
{
    public partial class ZMachineProfiler
    {
        private sealed class Routine : IRoutine
        {
            private readonly ZMachineProfiler profiler;
            private readonly int address;
            private readonly RoutineCompilationStatistics statistics;

            private List<int> callIndexes;
            private ReadOnlyCollection<ICall> calls;
            private TimeSpan inclusiveTime;
            private TimeSpan exclusiveTime;

            public Routine(ZMachineProfiler profiler, int address, RoutineCompilationStatistics statistics)
            {
                this.profiler = profiler;
                this.address = address;
                this.statistics = statistics;
                this.callIndexes = new List<int>();
            }

            public void AddCall(int index)
            {
                callIndexes.Add(index);
            }

            public void Done()
            {
                var callList = callIndexes.ConvertAll(i => (ICall)profiler.GetCallByIndex(i));
                callList.TrimExcess();
                calls = new ReadOnlyCollection<ICall>(callList);
                callIndexes = null;

                inclusiveTime = calls.Aggregate(TimeSpan.Zero, (r, c) => r + (c.Recursive ? TimeSpan.Zero : c.InclusiveTime));
                exclusiveTime = calls.Aggregate(TimeSpan.Zero, (r, c) => r + c.ExclusiveTime);
            }

            public int Address => address;

            public ReadOnlyCollection<ICall> Calls => calls;

            public TimeSpan InclusiveTime => inclusiveTime;

            public TimeSpan ExclusiveTime => exclusiveTime;

            public double InclusivePercentage => ((double)inclusiveTime.Ticks / (double)profiler.RunningTime.Ticks) * 100;

            public double ExclusivePercentage => ((double)exclusiveTime.Ticks / (double)profiler.RunningTime.Ticks) * 100;

            public int LocalCount => statistics.LocalCount;

            public int ZCodeInstructionCount => statistics.Routine.Instructions.Length;

            public int ILInstructionCount => statistics.OpcodeCount;

            public int ILByteSize => statistics.Size;

        }
    }
}
