using System.ComponentModel.Composition;
using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Collections;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class MessageLogViewModel : ViewModelWithViewBase<UserControl>, IMessageLog
    {
        private readonly StoryService storyService;
        private readonly BulkObservableCollection<MessageViewModel> messages;

        [ImportingConstructor]
        public MessageLogViewModel(
            StoryService storyService)
            : base("MessageLogView")
        {
            this.storyService = storyService;
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

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            DebuggerService.StateChanged += DebuggerService_StateChanged;
            DebuggerService.Processor.RegisterMessageLog(this);
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
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

        protected override void ViewCreated(UserControl view)
        {
            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;
        }

        public BulkObservableCollection<MessageViewModel> Messages
        {
            get { return messages; }
        }
    }
}
