using System.Windows.Controls;
using ZDebug.UI.Collections;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class CallStackViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly BulkObservableCollection<StackFrameViewModel> stackFrames;

        public CallStackViewModel()
            : base("CallStackView")
        {
            stackFrames = new BulkObservableCollection<StackFrameViewModel>();
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
                        stackFrames.Add(new StackFrameViewModel(frame));
                    }
                }
                finally
                {
                    stackFrames.EndBulkOperation();
                }
            }
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            Update();
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
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

        protected override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
            DebuggerService.ProcessorStepped += DebuggerService_ProcessorStepped;
        }

        public BulkObservableCollection<StackFrameViewModel> StackFrames
        {
            get { return stackFrames; }
        }
    }
}
