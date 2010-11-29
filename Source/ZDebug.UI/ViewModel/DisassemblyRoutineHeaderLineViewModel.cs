using ZDebug.Core.Instructions;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyRoutineHeaderLineViewModel : DisassemblyLineViewModel
    {
        private readonly Routine routine;

        public DisassemblyRoutineHeaderLineViewModel(Routine routine)
        {
            this.routine = routine;
        }

        public int Address
        {
            get { return routine.Address; }
        }
    }
}
