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

        public void NameUpdated()
        {
            PropertyChanged("Name");
        }

        public int Address
        {
            get { return routine.Address; }
        }

        public bool HasLocals
        {
            get { return routine.Locals.Count > 0; }
        }

        public string LocalCountText
        {
            get
            {
                var localCount = routine.Locals.Count;
                if (localCount == 1)
                {
                    return "1 local";
                }
                else
                {
                    return localCount + " locals";
                }
            }
        }

        public string Name
        {
            get { return routine.Name; }
        }
    }
}
