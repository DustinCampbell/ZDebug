using System;
using ZDebug.Core.Routines;

namespace ZDebug.UI.Services
{
    public sealed class RoutineNameChangedEventArgs : EventArgs
    {
        private readonly ZRoutine routine;

        public RoutineNameChangedEventArgs(ZRoutine routine)
        {
            this.routine = routine;
        }

        public ZRoutine Routine
        {
            get { return routine; }
        }
    }
}
