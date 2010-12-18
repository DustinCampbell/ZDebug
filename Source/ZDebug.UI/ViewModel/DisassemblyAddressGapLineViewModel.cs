
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
            get { return start.Address + start.Length - 1; }
        }

        public int EndAddress
        {
            get { return end.Address; }
        }

        public int Length
        {
            get { return EndAddress - StartAddress; }
        }

        public string LengthText
        {
            get
            {
                var length = EndAddress - StartAddress;
                if (length == 1)
                {
                    return "1 byte";
                }
                else
                {
                    return length + " bytes";
                }
            }
        }

    }
}
