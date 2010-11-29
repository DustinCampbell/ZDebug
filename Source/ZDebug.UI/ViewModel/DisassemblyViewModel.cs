using System.Windows.Controls;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly BulkObservableCollection<DisassemblyLineViewModel> lines;

        public DisassemblyViewModel()
            : base("DisassemblyView")
        {
            lines = new BulkObservableCollection<DisassemblyLineViewModel>();
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            var reader = e.Story.Memory.CreateReader(0);

            var blank = new DisassemblyBlankLineViewModel();

            lines.BeginBulkOperation();
            try
            {
                var routineTable = e.Story.RoutineTable;

                for (int rIndex = 0; rIndex < routineTable.Count; rIndex++)
                {
                    if (rIndex > 0)
                    {
                        lines.Add(blank);
                    }

                    var routine = routineTable[rIndex];

                    lines.Add(new DisassemblyRoutineHeaderLineViewModel(routine));

                    lines.Add(blank);

                    foreach (var i in routine.Instructions)
                    {
                        lines.Add(new DisassemblyInstructionLineViewModel(i));
                    }
                }
            }
            finally
            {
                lines.EndBulkOperation();
            }
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            lines.Clear();
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;
        }

        public BulkObservableCollection<DisassemblyLineViewModel> Lines
        {
            get { return lines; }
        }
    }
}
