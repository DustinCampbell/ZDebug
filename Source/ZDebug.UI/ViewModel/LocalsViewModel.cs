using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class LocalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly IndexedVariableViewModel[] locals;

        public LocalsViewModel()
            : base("LocalsView")
        {
            locals = new IndexedVariableViewModel[15];

            for (int i = 0; i < 15; i++)
            {
                locals[i] = new IndexedVariableViewModel(i, 0);
            }
        }

        private void Update()
        {
            if (DebuggerService.State != DebuggerState.Running)
            {
                var processor = DebuggerService.Story.Processor;
                var localCount = processor.LocalCount;

                for (int i = 0; i < 15; i++)
                {
                    var local = locals[i];

                    var visible = i < localCount;
                    if (visible)
                    {
                        local.IsModified = local.Value != processor.Locals[i];
                        local.Value = processor.Locals[i];
                    }

                    local.Visible = visible;

                    if (!visible)
                    {
                        local.IsModified = false;
                    }
                }
            }
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            Update();

            e.Story.Processor.Stepped += Processor_Stepped;
        }

        private void Processor_Stepped(object sender, ProcessorSteppedEventArgs e)
        {
            Update();
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            for (int i = 0; i < 15; i++)
            {
                locals[i].Visible = false;
            }

            e.Story.Processor.Stepped -= Processor_Stepped;
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.Running)
            {
                this.View.DataContext = null;
            }
            else if (e.OldState == DebuggerState.Running)
            {
                Update();
                this.View.DataContext = this;
            }
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
        }

        public IndexedVariableViewModel[] Locals
        {
            get { return locals; }
        }
    }
}
