using System;

namespace ZDebug.Terp.Profiling
{
    public interface ICall
    {
        Routine Routine { get; }
        int Index { get; }
        ICall Parent { get; }
        int ChildCount { get; }
        ICall this[int index] { get; }
        TimeSpan Elapsed { get; }
    }
}
