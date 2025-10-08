using ZDebug.Core.Routines;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyAddressGapLineViewModel : DisassemblyLineViewModel
    {
        private readonly ZRoutine start;
        private readonly ZRoutine end;

        public DisassemblyAddressGapLineViewModel(ZRoutine start, ZRoutine end)
        {
            this.start = start;
            this.end = end;
        }

        public ZRoutine Start => start;

        public ZRoutine End => end;

        public int StartAddress => start.Address + start.Length - 1;

        public int EndAddress => end.Address;

        public int Length => EndAddress - StartAddress;

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
