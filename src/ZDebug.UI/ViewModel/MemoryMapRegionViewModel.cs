using ZDebug.Core.Basics;

namespace ZDebug.UI.ViewModel;

internal sealed class MemoryMapRegionViewModel(MemoryMapRegion region) : ViewModelBase
{
    public string Name => region.Name;
    public int Base => region.Base;
    public int End => region.End;
    public int Size => region.Size;
}
