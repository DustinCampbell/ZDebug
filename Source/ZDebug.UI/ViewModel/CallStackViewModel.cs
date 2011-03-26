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

        private readonly BulkObservableCollection<StackFrameViewModel> stackFrames;

        [ImportingConstructor]
        public CallStackViewModel(
            StoryService storyService,
            RoutineService routineService)
            : base("CallStackView")
        {
            this.storyService = storyService;
            this.routineService = routineService;

            this.stackFrames = new BulkObservableCollection<StackFrameViewModel>();
        }

        private void Update()
        {
            if (DebuggerService.State != DebuggerState.Running)
            {
                var frames = DebuggerService.Processor.GetStackFrames();

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

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
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

        private void DebuggerService_ProcessorStepped(object sender, ProcessorSteppedEventArgs e)
        {
            if (DebuggerService.State != DebuggerState.Running)
            {
                Update();
            }
        }

        protected override void ViewCreated(UserControl view)
        {
            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
            DebuggerService.ProcessorStepped += DebuggerService_ProcessorStepped;
        }

        public BulkObservableCollection<StackFrameViewModel> StackFrames
        {
            get { return stackFrames; }
        }
    }
}
