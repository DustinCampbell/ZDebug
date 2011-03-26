using System.ComponentModel.Composition;
using System.Windows.Controls;
using ZDebug.UI.Collections;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class CallStackViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly RoutineService routineService;
        private readonly DebuggerService debuggerService;

        private readonly BulkObservableCollection<StackFrameViewModel> stackFrames;

        [ImportingConstructor]
        public CallStackViewModel(
            StoryService storyService,
            RoutineService routineService,
            DebuggerService debuggerService)
            : base("CallStackView")
        {
            this.storyService = storyService;
            this.routineService = routineService;
            this.debuggerService = debuggerService;

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

        private void DebuggerService_MachineInitialized(object sender, MachineInitializedEventArgs e)
        {
            Update();
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
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

        protected override void ViewCreated(UserControl view)
        {
            debuggerService.MachineInitialized += DebuggerService_MachineInitialized;
            storyService.StoryClosing += StoryService_StoryClosing;
            debuggerService.StateChanged += DebuggerService_StateChanged;
            debuggerService.Stepped += DebuggerService_ProcessorStepped;
        }

        public BulkObservableCollection<StackFrameViewModel> StackFrames
        {
            get { return stackFrames; }
        }
    }
}
