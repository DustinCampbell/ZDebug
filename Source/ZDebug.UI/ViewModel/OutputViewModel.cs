using System.Text;
using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class OutputViewModel : ViewModelWithViewBase<UserControl>, IOutputStream
    {
        private readonly StringBuilder builder;

        public OutputViewModel()
            : base("OutputView")
        {
            builder = new StringBuilder();
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            e.Story.Processor.OutputStreams.RegisterScreen(this);
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            builder.Clear();
        }

        public string Text
        {
            get { return builder.ToString(); }
        }

        void IOutputStream.Print(string text)
        {
            builder.Append(text);
            PropertyChanged("Text");
        }

        void IOutputStream.Print(char ch)
        {
            builder.Append(ch);
            PropertyChanged("Text");
        }
    }
}
