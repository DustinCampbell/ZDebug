using ZDebug.Core.Basics;

namespace ZDebug.UI.ViewModel;

internal sealed class MemoryMapRegionViewModel : ViewModelBase
{
    private readonly MemoryMapRegion region;

    public MemoryMapRegionViewModel(MemoryMapRegion region)
    {
        this.region = region;
    }

    public string Name => region.Name;

    public int Base => region.Base;

    public int End => region.End;

    public int Size => region.Size;
}
