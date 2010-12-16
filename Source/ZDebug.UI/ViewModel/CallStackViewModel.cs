using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

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

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            stackFrames.BeginBulkOperation();
            try
            {
                var mainFrame = new StackFrameViewModel(e.Story.Processor.PC - 1, callAddress: null);
                mainFrame.IsCurrent = true;
                stackFrames.Add(mainFrame);
            }
            finally
            {
                stackFrames.EndBulkOperation();
            }

            e.Story.Processor.EnterStackFrame += Processor_EnterFrame;
            e.Story.Processor.ExitStackFrame += Processor_ExitFrame;
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            stackFrames.Clear();

            e.Story.Processor.EnterStackFrame -= Processor_EnterFrame;
            e.Story.Processor.ExitStackFrame -= Processor_ExitFrame;
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.Running)
            {
                this.View.DataContext = null;
                stackFrames.BeginBulkOperation();
            }
            else if (e.OldState == DebuggerState.Running)
            {
                stackFrames.EndBulkOperation();
                this.View.DataContext = this;
            }
        }

        private void Processor_ExitFrame(object sender, StackFrameEventArgs e)
        {
            stackFrames.RemoveAt(0);
            stackFrames[0].IsCurrent = true;
        }

        private void Processor_EnterFrame(object sender, StackFrameEventArgs e)
        {
            stackFrames[0].IsCurrent = false;
            var newFrame = new StackFrameViewModel(e.Address, callAddress: DebuggerService.Story.Processor.ExecutingInstruction.Address);
            newFrame.IsCurrent = true;
            stackFrames.Insert(0, newFrame);
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
        }

        public BulkObservableCollection<StackFrameViewModel> StackFrames
        {
            get { return stackFrames; }
        }
    }
}
