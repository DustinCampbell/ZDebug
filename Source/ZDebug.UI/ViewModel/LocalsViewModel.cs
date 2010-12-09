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
        }

        private void Processor_EnterFrame(object sender, StackFrameEventArgs e)
        {
            Update(e.NewFrame);
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            locals.Clear();

            e.Story.Processor.EnterFrame -= Processor_EnterFrame;
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
        }

        public BulkObservableCollection<LocalVariableViewModel> Locals
        {
            get { return locals; }
        }
    }
}
