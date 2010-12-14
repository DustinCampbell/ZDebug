using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class MessageLogViewModel : ViewModelWithViewBase<UserControl>, IMessageLog
    {
        private readonly BulkObservableCollection<MessageViewModel> messages;

        public MessageLogViewModel()
            : base("MessageLogView")
        {
            messages = new BulkObservableCollection<MessageViewModel>();
        }

        public void SendError(string message)
        {
            messages.Add(MessageViewModel.CreateError(message));
        }

        public void SendWarning(string message)
        {
            messages.Add(MessageViewModel.CreateWarning(message));
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            DebuggerService.StateChanged += DebuggerService_StateChanged;
            e.Story.Processor.RegisterMessageLog(this);
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            messages.Clear();
            DebuggerService.StateChanged -= DebuggerService_StateChanged;
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.StoppedAtError)
            {
                SendError(DebuggerService.CurrentException.Message);
            }
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
        }

        public BulkObservableCollection<MessageViewModel> Messages
        {
            get { return messages; }
        }
    }
}
