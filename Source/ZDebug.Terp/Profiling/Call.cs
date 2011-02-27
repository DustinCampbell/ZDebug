using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ZDebug.Terp.Profiling
{
    public sealed class Call
    {
        private readonly Routine routine;
        private readonly Call parent;
        private readonly List<Call> children;
        private readonly ReadOnlyCollection<Call> readOnlyChildren;
        private readonly Stopwatch stopWatch;

        public Call(Routine routine, Call parent = null)
        {
            this.routine = routine;
            this.parent = parent;
            this.children = new List<Call>();
            this.readOnlyChildren = new ReadOnlyCollection<Call>(children);
            this.stopWatch = new Stopwatch();

            if (parent != null)
            {
                parent.children.Add(this);
            }
        }

        public void Enter()
        {
            stopWatch.Start();
        }

        public void Exit()
        {
            stopWatch.Stop();
        }

        public Routine Routine
        {
            get
            {
                return routine;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                return stopWatch.Elapsed;
            }
        }

        public Call Parent
        {
            get
            {
                return parent;
            }
        }

        public ReadOnlyCollection<Call> Children
        {
            get
            {
                return readOnlyChildren;
            }
        }
    }
}
