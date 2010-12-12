using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.IO.Windows;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class OutputViewModel : ViewModelWithViewBase<UserControl>, IScreen
    {
        private ZWindowManager windowManager;
        private Grid windowContainer;

        private ZWindow mainWindow;

        public OutputViewModel()
            : base("OutputView")
        {
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            windowManager = new ZWindowManager();
            windowContainer = this.View.FindName<Grid>("windowContainer");
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            mainWindow = windowManager.Open(ZWindowType.TextBuffer);
            windowContainer.Children.Add(mainWindow);

            windowManager.Activate(mainWindow);

            e.Story.Processor.RegisterScreen(this);
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            windowContainer.Children.Clear();
            windowManager.Root.Close();
        }

        void IOutputStream.Print(string text)
        {
            windowManager.ActiveWindow.PutString(text);
        }

        void IOutputStream.Print(char ch)
        {
            windowManager.ActiveWindow.PutChar(ch);
        }

        void IScreen.Clear(int window)
        {
            if (window == 0)
            {
                mainWindow.Clear();
            }
        }

        void IScreen.ClearAll(bool unsplit)
        {
            mainWindow.Clear();
        }

        void IScreen.SetTextStyle(ZTextStyle style)
        {
            windowManager.ActiveWindow.SetBold(style.HasFlag(ZTextStyle.Bold));
            windowManager.ActiveWindow.SetItalic(style.HasFlag(ZTextStyle.Italic));
            windowManager.ActiveWindow.SetFixedPitch(style.HasFlag(ZTextStyle.FixedPitch));
        }
    }
}
