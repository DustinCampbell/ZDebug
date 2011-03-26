using System.ComponentModel.Composition;
using System.Windows.Controls;
using ZDebug.UI.Collections;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class CallStackViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly DebuggerService debuggerService;
        private readonly RoutineService routineService;

        private readonly BulkObservableCollection<StackFrameViewModel> stackFrames;

        [ImportingConstructor]
        private CallStackViewModel(
            DebuggerService debuggerService,
            RoutineService routineService)
            : base("CallStackView")
        {
            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;
            this.debuggerService.MachineDestroyed += new System.EventHandler<MachineDestroyedEventArgs>(DebuggerService_MachineDestroyed);
            this.debuggerService.StateChanged += DebuggerService_StateChanged;
            this.debuggerService.Stepped += DebuggerService_ProcessorStepped;

            this.routineService = routineService;

            this.stackFrames = new BulkObservableCollection<StackFrameViewModel>();
        }

        private void Update()
        {
            if (debuggerService.State != DebuggerState.Running)
            {
                var frames = debuggerService.Machine.GetStackFrames();

                stackFrames.BeginBulkOperation();
                try
                {
                    stackFrames.Clear();

                    foreach (var frame in frames)
                    {
                        stackFrames.Add(new StackFrameViewModel(frame, routineService.RoutineTable));
                    }
                }
                finally
                {
                    stackFrames.EndBulkOperation();
                }
            }
        }

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            Update();
        }

        private void DebuggerService_MachineDestroyed(object sender, MachineDestroyedEventArgs e)
        {
            stackFrames.Clear();
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            // When input is wrapped up, we need to update as if the processor had stepped.
            if ((e.OldState == DebuggerState.AwaitingInput ||
                e.OldState == DebuggerState.Running) &&
                e.NewState != DebuggerState.Unavailable)
            {
                Update();
            }
        }

        private void DebuggerService_ProcessorStepped(object sender, SteppedEventArgs e)
        {
            if (debuggerService.State != DebuggerState.Running)
            {
                Update();
            }
        }

        public BulkObservableCollection<StackFrameViewModel> StackFrames
        {
            get { return stackFrames; }
        }
    }
}
