using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Controls;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class OutputViewModel : ViewModelWithViewBase<UserControl>, IScreen
    {
        private ZTextScreen screen;

        public OutputViewModel()
            : base("OutputView")
        {
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            screen = this.View.FindName<ZTextScreen>("screen");
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            e.Story.Processor.RegisterScreen(this);
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            screen.ClearAll(unsplit: true);
        }

        void IOutputStream.Print(string text)
        {
            screen.Print(text);
        }

        void IOutputStream.Print(char ch)
        {
            screen.Print(ch);
        }

        void IScreen.Clear(int window)
        {
            screen.Clear(window);
        }

        void IScreen.ClearAll(bool unsplit)
        {
            screen.ClearAll(unsplit);
        }
    }
}
