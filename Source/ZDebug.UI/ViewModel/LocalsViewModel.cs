using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class LocalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly BulkObservableCollection<LocalVariableViewModel> locals;

        public LocalsViewModel()
            : base("LocalsView")
        {
            locals = new BulkObservableCollection<LocalVariableViewModel>();
        }

        private void Update(StackFrame frame)
        {
            locals.BeginBulkOperation();
            try
            {
                locals.Clear();

                for (int i = 0; i < frame.Locals.Count; i++)
                {
                    locals.Add(new LocalVariableViewModel(i, frame.Locals[i]));
                }
            }
            finally
            {
                locals.EndBulkOperation();
            }
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            Update(e.Story.Processor.CurrentFrame);

            e.Story.Processor.EnterFrame += Processor_EnterFrame;
            e.Story.Processor.LocalVariableChanged += Processor_LocalVariableChanged;
            e.Story.Processor.Stepping += Processor_Stepping;
        }

        private void Processor_EnterFrame(object sender, StackFrameEventArgs e)
        {
            Update(e.NewFrame);
        }

        private void Processor_LocalVariableChanged(object sender, LocalVariableChangedEventArgs e)
        {
            var viewModel = locals[e.Index];
            viewModel.Value = e.NewValue;
            viewModel.IsModified = true;
        }

        private void Processor_Stepping(object sender, ProcessorSteppingEventArgs e)
        {
            foreach (var local in locals)
            {
                local.IsModified = false;
            }
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            locals.Clear();

            e.Story.Processor.EnterFrame -= Processor_EnterFrame;
            e.Story.Processor.LocalVariableChanged -= Processor_LocalVariableChanged;
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.Running)
            {
                foreach (var local in locals)
                {
                    local.IsModified = false;
                    local.IsFrozen = true;
                }
            }
            else
            {
                foreach (var local in locals)
                {
                    local.IsFrozen = false;
                }
            }
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
        }

        public BulkObservableCollection<LocalVariableViewModel> Locals
        {
            get { return locals; }
        }
    }
}
