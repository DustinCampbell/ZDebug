using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;

namespace ZDebug.UI.Services
{
    public sealed class RoutineNameChangedEventArgs : EventArgs
    {
        private readonly Routine routine;

        public RoutineNameChangedEventArgs(Routine routine)
        {
            this.routine = routine;
        }

        public Routine Routine
        {
            get { return routine; }
        }
    }
}
