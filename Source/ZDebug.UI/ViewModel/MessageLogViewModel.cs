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
        private readonly DebuggerService debuggerService;

        private readonly BulkObservableCollection<MessageViewModel> messages;

        [ImportingConstructor]
        public MessageLogViewModel(
            StoryService storyService,
            DebuggerService debuggerService)
            : base("MessageLogView")
        {
            this.storyService = storyService;
            this.debuggerService = debuggerService;

            this.messages = new BulkObservableCollection<MessageViewModel>();
        }

        public void SendError(string message)
        {
            messages.Add(MessageViewModel.CreateError(message));
        }

        public void SendWarning(string message)
        {
            messages.Add(MessageViewModel.CreateWarning(message));
        }

        private void DebuggerService_MachineInitialized(object sender, MachineInitializedEventArgs e)
        {
            debuggerService.StateChanged += DebuggerService_StateChanged;
            debuggerService.Machine.RegisterMessageLog(this);
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            messages.Clear();
            debuggerService.StateChanged -= DebuggerService_StateChanged;
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.StoppedAtError)
            {
                SendError(debuggerService.CurrentException.Message);
            }
        }

        protected override void ViewCreated(UserControl view)
        {
            debuggerService.MachineInitialized += DebuggerService_MachineInitialized;
            storyService.StoryClosing += StoryService_StoryClosing;
        }

        public BulkObservableCollection<MessageViewModel> Messages
        {
            get { return messages; }
        }
    }
}
