using System.ComponentModel.Composition;
using System.Windows.Media.Imaging;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    public sealed class GameInfoDialogViewModel : DialogViewModelBase
    {
        private readonly StoryService storyService;

        [ImportingConstructor]
        private GameInfoDialogViewModel(StoryService storyService)
            : base("GameInfoDialogView")
        {
            this.storyService = storyService;
        }

        public string Title
        {
            get { return storyService.GameInfo.Title; }
        }

        public string Headline
        {
            get { return storyService.GameInfo.Headline; }
        }

        public string Author
        {
            get { return storyService.GameInfo.Author; }
        }

        public string FirstPublished
        {
            get { return storyService.GameInfo.FirstPublished; }
        }

        public string Description
        {
            get { return storyService.GameInfo.Description; }
        }

        public BitmapSource Cover
        {
            get { return storyService.GameInfo.Cover; }
        }
    }
}
