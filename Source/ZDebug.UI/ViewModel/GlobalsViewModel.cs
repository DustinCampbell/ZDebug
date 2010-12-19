using System.Windows.Controls;
using ZDebug.Core.Execution;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    internal sealed class GlobalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly IndexedVariableViewModel[] globals;

        public GlobalsViewModel()
            : base("GlobalsView")
        {
            globals = new IndexedVariableViewModel[240];

            for (int i = 0; i < 240; i++)
            {
                var newGlobal = new IndexedVariableViewModel(i, 0);
                newGlobal.Visible = false;
                globals[i] = newGlobal;
            }
        }

        private void Update(bool storyOpened = false)
        {
            if (DebuggerService.State != DebuggerState.Running)
            {
                var processor = DebuggerService.Story.Processor;

                // Update globals...
                for (int i = 0; i < 240; i++)
                {
                    var global = globals[i];

                    var newGlobalValue = DebuggerService.Story.GlobalVariablesTable[i];
                    global.IsModified = !storyOpened && global.Value != newGlobalValue;
                    global.Value = newGlobalValue;
                }
            }
        }

        private void Processor_Stepped(object sender, ProcessorSteppedEventArgs e)
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
            for (int i = 0; i < 240; i++)
            {
                globals[i].Visible = false;
            }

            e.Story.Processor.Stepped -= Processor_Stepped;
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            for (int i = 0; i < 240; i++)
            {
                globals[i].Visible = true;
            }

            Update(storyOpened: true);

            e.Story.Processor.Stepped += Processor_Stepped;
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
        }

        public IndexedVariableViewModel[] Globals
        {
            get { return globals; }
        }
    }
}
