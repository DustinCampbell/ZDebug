using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class LocalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly LocalVariableViewModel[] locals;

        public LocalsViewModel()
            : base("LocalsView")
        {
            locals = new LocalVariableViewModel[15];

            for (int i = 0; i < 15; i++)
            {
                locals[i] = new LocalVariableViewModel(i, 0);
            }
        }

        private void Update()
        {
            if (DebuggerService.State == DebuggerState.Running)
            {
                for (int i = 0; i < 15; i++)
                {
                    locals[i].IsModified = false;
                }
            }
            else
            {
                var processor = DebuggerService.Story.Processor;
                var localCount = processor.LocalCount;

                for (int i = 0; i < 15; i++)
                {
                    var local = locals[i];

                    var current = i < localCount;
                    if (current)
                    {
                        local.Value = processor.Locals[i];
                    }

                    local.Visible = current;
                    local.IsModified = false;
                }
            }
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            Update();

            e.Story.Processor.EnterStackFrame += Processor_EnterFrame;
            e.Story.Processor.ExitStackFrame += Processor_ExitFrame;
            e.Story.Processor.LocalVariableChanged += Processor_LocalVariableChanged;
            e.Story.Processor.Stepping += Processor_Stepping;
        }

        private void Processor_EnterFrame(object sender, StackFrameEventArgs e)
        {
            Update();
        }

        private void Processor_ExitFrame(object sender, StackFrameEventArgs e)
        {
            Update();
        }

        private void Processor_LocalVariableChanged(object sender, LocalVariableChangedEventArgs e)
        {
            var viewModel = locals[e.Index];
            viewModel.Value = e.NewValue;
            viewModel.IsModified = true;
        }

        private void Processor_Stepping(object sender, ProcessorSteppingEventArgs e)
        {
            if (DebuggerService.State != DebuggerState.Running)
            {
                for (int i = 0; i < 15; i++)
                {
                    locals[i].IsModified = false;
                }
            }
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            for (int i = 0; i < 15; i++)
            {
                locals[i].Visible = false;
            }

            e.Story.Processor.EnterStackFrame -= Processor_EnterFrame;
            e.Story.Processor.ExitStackFrame -= Processor_ExitFrame;
            e.Story.Processor.LocalVariableChanged -= Processor_LocalVariableChanged;
            e.Story.Processor.Stepping -= Processor_Stepping;
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

            if (e.NewState == DebuggerState.Running)
            {
                for (int i = 0; i < 15; i++)
                {
                    var local = locals[i];
                    local.IsModified = false;
                    local.IsFrozen = true;
                }
            }
            else
            {
                for (int i = 0; i < 15; i++)
                {
                    locals[i].IsFrozen = false;
                }
            }
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
        }

        public LocalVariableViewModel[] Locals
        {
            get { return locals; }
        }
    }
}
