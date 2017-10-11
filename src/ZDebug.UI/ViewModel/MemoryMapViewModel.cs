using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Composition;
using System.Windows.Controls;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal class MemoryMapViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private ReadOnlyCollection<MemoryMapRegionViewModel> regions;

        [ImportingConstructor]
        public MemoryMapViewModel(
            StoryService storyService)
            : base("MemoryMapView")
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosing += StoryService_StoryClosing;
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
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

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            regions = null;

            PropertyChanged("Regions");
            PropertyChanged("HasStory");
        }

        public ReadOnlyCollection<MemoryMapRegionViewModel> Regions
        {
            get { return regions; }
        }

        public bool HasStory
        {
            get { return storyService.IsStoryOpen; }
        }
    }
}
