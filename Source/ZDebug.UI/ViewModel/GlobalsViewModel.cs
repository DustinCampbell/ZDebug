using System.ComponentModel.Composition;
using System.Windows.Controls;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export]
    internal sealed class GlobalsViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly DebuggerService debuggerService;

        private readonly IndexedVariableViewModel[] globals;

        [ImportingConstructor]
        public GlobalsViewModel(
            StoryService storyService,
            DebuggerService debuggerService)
            : base("GlobalsView")
        {
            this.storyService = storyService;
            this.debuggerService = debuggerService;

            this.globals = new IndexedVariableViewModel[240];

            for (int i = 0; i < 240; i++)
            {
                var newGlobal = new IndexedVariableViewModel(i, 0);
                newGlobal.Visible = false;
                globals[i] = newGlobal;
            }
        }

        private void Update(bool storyOpened = false)
        {
            if (debuggerService.State != DebuggerState.Running)
            {
                var story = storyService.Story;

                // Update globals...
                for (int i = 0; i < 240; i++)
                {
                    var global = globals[i];

                    var newGlobalValue = story.GlobalVariablesTable[i];
                    global.IsModified = !storyOpened && global.Value != newGlobalValue;
                    global.Value = newGlobalValue;
                }
            }
        }

        private void DebuggerService_ProcessorStepped(object sender, SteppedEventArgs e)
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

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            for (int i = 0; i < 240; i++)
            {
                globals[i].Visible = false;
            }
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            for (int i = 0; i < 240; i++)
            {
                globals[i].Visible = true;
            }

            Update(storyOpened: true);
        }

        protected override void ViewCreated(UserControl view)
        {
            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;
            debuggerService.StateChanged += DebuggerService_StateChanged;
            debuggerService.Stepped += DebuggerService_ProcessorStepped;
        }

        public IndexedVariableViewModel[] Globals
        {
            get { return globals; }
        }
    }
}
