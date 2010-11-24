using System;
using System.Windows.Controls;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal partial class MemoryViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly BulkObservableCollection<MemoryLineViewModel> lines;

        public MemoryViewModel()
            : base("MemoryView")
        {
            lines = new BulkObservableCollection<MemoryLineViewModel>();
        }

        private void StoryOpened(object sender, StoryEventArgs e)
        {
            var reader = e.Story.Memory.CreateReader(0);

            lines.BeginBulkOperation();
            try
            {
                while (reader.RemainingBytes > 0)
                {
                    var address = reader.Index;
                    var count = Math.Min(8, reader.RemainingBytes);
                    var values = reader.NextWords(count);

                    lines.Add(new MemoryLineViewModel(address, values));
                }
            }
            finally
            {
                lines.EndBulkOperation();
            }
        }

        private void StoryClosed(object sender, StoryEventArgs e)
        {
            lines.Clear();
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += StoryOpened;
            DebuggerService.StoryClosed += StoryClosed;
        }

        public BulkObservableCollection<MemoryLineViewModel> Lines
        {
            get { return lines; }
        }
    }
}
