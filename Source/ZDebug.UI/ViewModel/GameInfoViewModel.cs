using System.Windows;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class GameInfoViewModel : ViewModelWithViewBase<Window>
    {
        public GameInfoViewModel()
            : base("GameInfoView")
        {
        }

        public string Title
        {
            get { return DebuggerService.GameInfo.Title; }
        }

        public string Headline
        {
            get { return DebuggerService.GameInfo.Headline; }
        }

        public string Author
        {
            get { return DebuggerService.GameInfo.Author; }
        }

        public string FirstPublished
        {
            get { return DebuggerService.GameInfo.FirstPublished; }
        }

        public string Description
        {
            get { return DebuggerService.GameInfo.Description; }
        }
    }
}
