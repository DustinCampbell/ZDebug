using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal class MemoryMapViewModel : ViewModelWithViewBase<UserControl>
    {
        private ReadOnlyCollection<MemoryMapRegionViewModel> regions;

        public MemoryMapViewModel()
            : base("MemoryMapView")
        {
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            var list = new List<MemoryMapRegionViewModel>();

            foreach (var region in e.Story.MemoryMap)
            {
                list.Add(new MemoryMapRegionViewModel(region));
            }

            regions = new ReadOnlyCollection<MemoryMapRegionViewModel>(list);

            PropertyChanged("Regions");
            PropertyChanged("HasStory");
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            regions = null;

            PropertyChanged("Regions");
            PropertyChanged("HasStory");
        }

        protected override void ViewCreated(UserControl view)
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
        }

        public ReadOnlyCollection<MemoryMapRegionViewModel> Regions
        {
            get { return regions; }
        }

        public bool HasStory
        {
            get { return DebuggerService.HasStory; }
        }
    }
}
