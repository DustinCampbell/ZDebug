using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ZDebug.Terp.Profiling
{
    public partial class ZMachineProfiler
    {
        private sealed class Call : ICall
        {
            private readonly ZMachineProfiler profiler;
            private readonly Routine routine;
            private readonly int index;
            private readonly int parentIndex;
            private readonly List<int> childIndexes;

            private Stopwatch stopwatch;
            private TimeSpan elapsed;

            public Call(ZMachineProfiler profiler, Routine routine, int index, int parentIndex)
            {
                this.profiler = profiler;
                this.routine = routine;
                this.index = index;
                this.parentIndex = parentIndex;
                this.childIndexes = new List<int>();

                if (parentIndex >= 0)
                {
                    profiler.GetCallByIndex(parentIndex).childIndexes.Add(index);
                }
            }

            public void Enter()
            {
                stopwatch = Stopwatch.StartNew();
            }

            public void Exit()
            {
                stopwatch.Stop();
                childIndexes.TrimExcess();
                elapsed = stopwatch.Elapsed;
                stopwatch = null;
            }

            public Routine Routine
            {
                get
                {
                    return routine;
                }
            }

            public int Index
            {
                get
                {
                    return index;
                }
            }

            public ICall Parent
            {
                get
                {
                    return profiler.GetCallByIndex(parentIndex);
                }
            }

            public int ChildCount
            {
                get
                {
                    return childIndexes.Count;
                }
            }

            public ICall this[int index]
            {
                get
                {
                    return profiler.GetCallByIndex(childIndexes[index]);
                }
            }

            public TimeSpan Elapsed
            {
                get
                {
                    return stopwatch == null
                        ? elapsed
                        : stopwatch.Elapsed;
                }
            }
        }
    }
}
