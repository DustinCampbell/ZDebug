using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            private List<int> childIndexes;
            private ReadOnlyCollection<ICall> children;

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
                var childList = childIndexes.ConvertAll(i => (ICall)profiler.GetCallByIndex(i));
                children = new ReadOnlyCollection<ICall>(childList);
                childIndexes = null;

                elapsed = stopwatch.Elapsed;
                stopwatch = null;
            }

            public IRoutine Routine
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
                    return parentIndex >= 0
                        ? profiler.GetCallByIndex(parentIndex)
                        : null;
                }
            }

            public ReadOnlyCollection<ICall> Children
            {
                get
                {
                    return children;
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
