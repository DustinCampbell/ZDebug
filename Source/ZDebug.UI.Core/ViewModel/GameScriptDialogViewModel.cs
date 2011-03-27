using System.ComponentModel.Composition;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    public sealed class GameScriptDialogViewModel : DialogViewModelBase
    {
        private readonly GameScriptService gameScriptService;

        private string commands;

        [ImportingConstructor]
        private GameScriptDialogViewModel(
            GameScriptService gameScriptService)
            : base("GameScriptDialogView")
        {
            this.gameScriptService = gameScriptService;
            this.gameScriptService.Reset += GameScriptService_Reset;

            commands = string.Join("\r\n", gameScriptService.Commands);
        }

        private void GameScriptService_Reset(object sender, ResetEventArgs e)
        {
            commands = string.Join("\r\n", gameScriptService.Commands);
        }

        public string Commands
        {
            get { return commands; }
            set { commands = value; }
        }
    }
}
