using System.ComponentModel.Composition;
using System.Windows;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class GameScriptDialogViewModel : ViewModelWithViewBase<Window>
    {
        private readonly GameScriptService gameScriptService;

        private string commands;

        [ImportingConstructor]
        public GameScriptDialogViewModel(
            GameScriptService gameScriptService)
            : base("GameScriptDialogView")
        {
            this.gameScriptService = gameScriptService;

            commands = string.Join("\r\n", gameScriptService.Commands);
        }

        public string Commands
        {
            get { return commands; }
            set { commands = value; }
        }
    }
}
