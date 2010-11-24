using System;
using System.Windows.Controls;
using ZDebug.Core.Basics;
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

        private void MemoryChanged(object sender, MemoryChangedEventArgs e)
        {
            // Replace affected lines
            int firstLineIndex = e.Address / 16;
            int lastLineIndex = (e.Address + e.Length) / 16;

            var reader = e.Memory.CreateReader(firstLineIndex * 16);

            for (int i = firstLineIndex; i <= lastLineIndex; i++)
            {
                var address = reader.Address;
                var count = Math.Min(8, reader.RemainingBytes);
                var values = reader.NextWords(count);

                lines[i] = new MemoryLineViewModel(address, values);
            }

            // TODO: Highlight modified memory
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            var reader = e.Story.Memory.CreateReader(0);

            lines.BeginBulkOperation();
            try
            {
                while (reader.RemainingBytes > 0)
                {
                    var address = reader.Address;
                    var count = Math.Min(8, reader.RemainingBytes);
                    var values = reader.NextWords(count);

                    lines.Add(new MemoryLineViewModel(address, values));
                }
            }
            finally
            {
                lines.EndBulkOperation();
            }

            e.Story.Memory.MemoryChanged += MemoryChanged;

            PropertyChanged("HasStory");
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            lines.Clear();

            PropertyChanged("HasStory");
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            var memoryChartContainer = this.View.FindName<Border>("memoryChartContainer");
            memoryChartContainer.Child = ViewModelWithView.Create<MemoryMapViewModel, UserControl>();
        }

        public bool HasStory
        {
            get { return DebuggerService.HasStory; }
        }

        public BulkObservableCollection<MemoryLineViewModel> Lines
        {
            get { return lines; }
        }
    }
}
