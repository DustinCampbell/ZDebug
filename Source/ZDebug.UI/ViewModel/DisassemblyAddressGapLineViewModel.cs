
using ZDebug.Core.Instructions;
namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyAddressGapLineViewModel : DisassemblyLineViewModel
    {
        private readonly Routine start;
        private readonly Routine end;

        public DisassemblyAddressGapLineViewModel(Routine start, Routine end)
        {
            this.start = start;
            this.end = end;
        }

        public Routine Start
        {
            get { return start; }
        }

        public Routine End
        {
            get { return end; }
        }

        public int StartAddress
        {
            get { return start.Address + start.Length; }
        }

        public int EndAddress
        {
            get { return end.Address - 1; }
        }
    }
}
