using System.Composition;
using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Collections;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class MessageLogViewModel : ViewModelWithViewBase<UserControl>, IMessageLog
    {
        private readonly DebuggerService debuggerService;
        private readonly BulkObservableCollection<MessageViewModel> messages;

        [ImportingConstructor]
        public MessageLogViewModel(DebuggerService debuggerService)
            : base("MessageLogView")
        {
            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;
            this.debuggerService.MachineDestroyed += DebuggerService_MachineDestroyed;

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

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            debuggerService.StateChanged += DebuggerService_StateChanged;
            debuggerService.Machine.RegisterMessageLog(this);
        }

        private void DebuggerService_MachineDestroyed(object sender, MachineDestroyedEventArgs e)
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

        public BulkObservableCollection<MessageViewModel> Messages
        {
            get { return messages; }
        }
    }
}
