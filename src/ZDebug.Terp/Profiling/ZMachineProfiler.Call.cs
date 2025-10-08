using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ZDebug.Terp.Profiling;

public partial class ZMachineProfiler
{
    private sealed class Call : ICall
    {
        private readonly ZMachineProfiler profiler;
        private readonly Routine routine;
        private readonly int index;
        private readonly int parentIndex;
        private readonly bool recursive;
        private List<int> childIndexes;
        private ReadOnlyCollection<ICall> children;

        private Stopwatch stopwatch;
        private TimeSpan inclusiveTime;
        private TimeSpan exclusiveTime;

        public Call(ZMachineProfiler profiler, Routine routine, int index, int parentIndex, bool recursive)
        {
            this.profiler = profiler;
            this.routine = routine;
            this.index = index;
            this.parentIndex = parentIndex;
            this.recursive = recursive;
            childIndexes = [];

            routine.AddCall(index);

            if (parentIndex >= 0)
            {
                profiler.GetCallByIndex(parentIndex).childIndexes.Add(index);
            }
        }

        public void Enter() => stopwatch = Stopwatch.StartNew();

        public void Exit()
        {
            stopwatch.Stop();

            var childList = childIndexes.ConvertAll(i => (ICall)profiler.GetCallByIndex(i));
            childList.TrimExcess();
            childList.Sort((c1, c2) => c1.InclusiveTime.CompareTo(c2.InclusiveTime));
            childList.Reverse();
            children = new ReadOnlyCollection<ICall>(childList);
            childIndexes = null;

            inclusiveTime = stopwatch.Elapsed;
            exclusiveTime = inclusiveTime - children.Aggregate(TimeSpan.Zero, (r, c) => r + c.InclusiveTime);
            stopwatch = null;
        }

        public IRoutine Routine => routine;

        public int Index => index;

        public ICall Parent
        {
            get => parentIndex >= 0
                  ? profiler.GetCallByIndex(parentIndex)
                  : null;
        }

        public ReadOnlyCollection<ICall> Children => children;

        public TimeSpan InclusiveTime => inclusiveTime;

        public TimeSpan ExclusiveTime => exclusiveTime;

        public double InclusivePercentage
        {
            get => parentIndex >= 0
                  ? (double)inclusiveTime.Ticks / (double)profiler.GetCallByIndex(parentIndex).InclusiveTime.Ticks * 100
                  : 100.0;
        }

        public double ExclusivePercentage => (double)exclusiveTime.Ticks / (double)profiler.GetCallByIndex(parentIndex).InclusiveTime.Ticks * 100;

        public bool Recursive => recursive;
    }
}
