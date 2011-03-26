using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using ZDebug.Core.Basics;
using ZDebug.UI.Collections;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    // TODO: This is officially broken and needs updating.
    [Export]
    internal partial class MemoryViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly StoryService storyService;
        private readonly BulkObservableCollection<MemoryLineViewModel> lines;

        [ImportingConstructor]
        public MemoryViewModel(
            StoryService storyService)
            : base("MemoryView")
        {
            this.storyService = storyService;
            this.lines = new BulkObservableCollection<MemoryLineViewModel>();
        }

        //private void MemoryChanged(object sender, MemoryEventArgs e)
        //{
        //    // Replace affected lines
        //    int firstLineIndex = e.Address / 16;
        //    int lastLineIndex = (e.Address + e.Length) / 16;

        //    var reader = e.Memory.CreateReader(firstLineIndex * 16);

        //    for (int i = firstLineIndex; i <= lastLineIndex; i++)
        //    {
        //        var address = reader.Address;
        //        var count = Math.Min(8, reader.RemainingBytes);
        //        var values = reader.NextWords(count);

        //        lines[i] = new MemoryLineViewModel(address, values);
        //    }

        //    // TODO: Highlight modified memory
        //}

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            var reader = new MemoryReader(e.Story.Memory, 0);

            lines.BeginBulkOperation();
            try
            {
                while (reader.RemainingBytes > 0)
                {
                    var address = reader.Address;

                    ushort[] values;

                    if (reader.RemainingBytes >= 16 || reader.RemainingBytes % 2 == 0)
                    {
                        var count = Math.Min(8, reader.RemainingBytes / 2);
                        values = reader.NextWords(count);
                    }
                    else
                    {
                        // if the last line is an odd number of bytes...

                        // TODO: memory view always shows even number of bytes
                        // (padding with zeroes if necessry). Need to fix it to show odd
                        // number of bytes if that's the case.
                        var valueList = new List<ushort>();
                        while (reader.RemainingBytes > 0)
                        {
                            if (reader.RemainingBytes > 2)
                            {
                                valueList.Add(reader.NextWord());
                            }
                            else
                            {
                                valueList.Add((ushort)(reader.NextByte() << 8));
                            }
                        }

                        values = valueList.ToArray();
                    }

                    lines.Add(new MemoryLineViewModel(address, values));
                }
            }
            finally
            {
                lines.EndBulkOperation();
            }

            PropertyChanged("HasStory");
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            lines.Clear();

            PropertyChanged("HasStory");
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.Running)
            {
                this.View.DataContext = null;
            }
            else
            {
                this.View.DataContext = this;
            }
        }

        protected override void ViewCreated(UserControl view)
        {
            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;
            DebuggerService.StateChanged += DebuggerService_StateChanged;
        }

        public bool HasStory
        {
            get { return storyService.IsStoryOpen; }
        }

        public BulkObservableCollection<MemoryLineViewModel> Lines
        {
            get { return lines; }
        }
    }
}
