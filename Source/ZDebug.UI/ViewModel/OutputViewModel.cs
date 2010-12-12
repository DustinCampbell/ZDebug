using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;
using ZDebug.UI.Windows;

namespace ZDebug.UI.ViewModel
{
    internal sealed class OutputViewModel : ViewModelWithViewBase<UserControl>, IScreen
    {
        private ZWindowManager windowManager;
        private ZTextBufferWindow lowerWindow;

        public OutputViewModel()
            : base("OutputView")
        {
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            windowManager = new ZWindowManager();
            var textBufferWindow = windowManager.Open(ZWindowType.TextBuffer) as ZTextBufferWindow;
            var windowContainer = this.View.FindName<Grid>("windowContainer");
            windowContainer.Children.Add(textBufferWindow);

            lowerWindow = textBufferWindow;
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            e.Story.Processor.RegisterScreen(this);
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            lowerWindow.Clear();
        }

        void IOutputStream.Print(string text)
        {
            lowerWindow.Print(text);
        }

        void IOutputStream.Print(char ch)
        {
            lowerWindow.Print(ch);
        }

        void IScreen.Clear(int window)
        {
        }

        void IScreen.ClearAll(bool unsplit)
        {
        }
    }
}
