using System;

namespace ZDebug.Core.Instructions
{
    public sealed class RoutineAddedEventArgs : EventArgs
    {
        private readonly Routine routine;

        public RoutineAddedEventArgs(Routine routine)
        {
            this.routine = routine;
        }

        public Routine Routine
        {
            get { return routine; }
        }
    }
}
