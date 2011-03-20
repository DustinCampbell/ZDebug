using System;
using System.Windows.Controls;
using ZDebug.Core.Extensions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class LocalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly IndexedVariableViewModel[] locals;

        private VariableViewModel[] stack;
        private VariableViewModel[] reversedStack;

        public LocalsViewModel()
            : base("LocalsView")
        {
            locals = new IndexedVariableViewModel[15];

            for (int i = 0; i < 15; i++)
            {
                locals[i] = new IndexedVariableViewModel(i, 0);
            }

            stack = new VariableViewModel[0];
            reversedStack = new VariableViewModel[0];
        }

        private void Update()
        {
            if (DebuggerService.State != DebuggerState.Running)
            {
                var processor = DebuggerService.Processor;

                // Update locals...
                var localCount = processor.LocalCount;
                for (int i = 0; i < 15; i++)
                {
                    var local = locals[i];

                    var visible = i < localCount;
                    if (visible)
                    {
                        local.IsModified = local.Value != processor.Locals[i] && local.Visible == visible;
                        local.Value = processor.Locals[i];
                    }

                    local.Visible = visible;

                    if (!visible)
                    {
                        local.IsModified = false;
                    }
                }

                // Update stack...
                var stackValues = processor.GetStackValues();
                Array.Resize(ref stack, stackValues.Length);
                var numItems = stackValues.Length - 1;
                for (int i = numItems; i >= 0; i--)
                {
                    int index = numItems - i;
                    if (stack[index] == null)
                    {
                        var stackValue = new VariableViewModel(stackValues[i]);
                        stackValue.IsModified = true;
                        stack[index] = stackValue;
                    }
                    else
                    {
                        var stackValue = stack[index];
                        stackValue.IsModified = stackValue.Value != stackValues[i];
                        stackValue.Value = stackValues[i];
                    }
                }

                reversedStack = stack.Reverse();
                PropertyChanged("LocalStack");
            }
        }

        private void DebuggerService_ProcessorStepped(object sender, ProcessorSteppedEventArgs e)
        {
            Update();
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

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            for (int i = 0; i < 15; i++)
            {
                locals[i].Visible = false;
            }

            PropertyChanged("HasStory");
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            Update();

            PropertyChanged("HasStory");
        }

        protected override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
            DebuggerService.ProcessorStepped += DebuggerService_ProcessorStepped;
        }

        public IndexedVariableViewModel[] Locals
        {
            get { return locals; }
        }

        public VariableViewModel[] LocalStack
        {
            get { return reversedStack; }
        }

        public bool HasStory
        {
            get { return DebuggerService.HasStory; }
        }
    }
}
