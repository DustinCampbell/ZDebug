using System.Windows;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class GameScriptDialogViewModel : ViewModelWithViewBase<Window>
    {
        private string commands;

        public GameScriptDialogViewModel()
            : base("GameScriptDialogView")
        {
            commands = string.Join("\r\n", DebuggerService.GetGameScriptCommands());
        }

        public string Commands
        {
            get { return commands; }
            set { commands = value; }
        }
    }
}
