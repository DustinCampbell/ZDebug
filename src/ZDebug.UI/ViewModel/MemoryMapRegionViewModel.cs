using ZDebug.Core.Basics;

namespace ZDebug.UI.ViewModel
{
    internal sealed class MemoryMapRegionViewModel : ViewModelBase
    {
        private readonly MemoryMapRegion region;

        public MemoryMapRegionViewModel(MemoryMapRegion region)
        {
            this.region = region;
        }

        public string Name
        {
            get { return region.Name; }
        }

        public int Base
        {
            get { return region.Base; }
        }

        public int End
        {
            get { return region.End; }
        }

        public int Size
        {
            get { return region.Size; }
        }
    }
}
