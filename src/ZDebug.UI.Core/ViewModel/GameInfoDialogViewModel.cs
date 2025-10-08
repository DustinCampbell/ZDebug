using System.Composition;
using System.Windows.Media.Imaging;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    public sealed class GameInfoDialogViewModel : DialogViewModelBase
    {
        private readonly StoryService storyService;

        [ImportingConstructor]
        public GameInfoDialogViewModel(StoryService storyService)
            : base("GameInfoDialogView")
        {
            this.storyService = storyService;
        }

        public string Title => storyService.GameInfo.Title;

        public string Headline => storyService.GameInfo.Headline;

        public string Author => storyService.GameInfo.Author;

        public string FirstPublished => storyService.GameInfo.FirstPublished;

        public string Description => storyService.GameInfo.Description;

        public BitmapSource Cover => storyService.GameInfo.Cover;
    }
}
