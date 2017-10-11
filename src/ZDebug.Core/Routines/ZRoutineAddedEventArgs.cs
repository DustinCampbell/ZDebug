using System;

namespace ZDebug.Core.Routines
{
    public sealed class ZRoutineAddedEventArgs : EventArgs
    {
        private readonly ZRoutine routine;

        public ZRoutineAddedEventArgs(ZRoutine routine)
        {
            this.routine = routine;
        }

        public ZRoutine Routine
        {
            get { return routine; }
        }
    }
}
