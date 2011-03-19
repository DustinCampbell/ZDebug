using System;
using System.Collections.ObjectModel;

namespace ZDebug.Terp.Profiling
{
    public interface ICall
    {
        IRoutine Routine { get; }

        int Index { get; }
        ICall Parent { get; }
        ReadOnlyCollection<ICall> Children { get; }

        TimeSpan InclusiveTime { get; }
        TimeSpan ExclusiveTime { get; }

        double InclusivePercentage { get; }
        double ExclusivePercentage { get; }

        bool Recursive { get; }
    }
}
