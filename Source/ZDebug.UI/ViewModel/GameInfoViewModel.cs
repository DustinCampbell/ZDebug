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
    }
}
