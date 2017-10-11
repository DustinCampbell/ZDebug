using System;
using System.Composition;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    public sealed class GameScriptDialogViewModel : DialogViewModelBase
    {
        private readonly GameScriptService gameScriptService;

        private string commands;

        [ImportingConstructor]
        public GameScriptDialogViewModel(
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

        protected override void OnDialogShown(bool? result)
        {
            if (result == true)
            {
                this.gameScriptService.SetCommands(
                    commands.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));
            }
        }

        public string Commands
        {
            get { return commands; }
            set { commands = value; }
        }
    }
}
