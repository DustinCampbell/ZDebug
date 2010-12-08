using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZDebug.Core.Execution;
using ZDebug.UI.Controls;
using ZDebug.UI.Services;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.ViewModel
{
    internal sealed class DisassemblyViewModel : ViewModelWithViewBase<UserControl>
    {
        private readonly BulkObservableCollection<DisassemblyLineViewModel> lines;
        private readonly Dictionary<int, DisassemblyLineViewModel> addressToLineMap;

        public DisassemblyViewModel()
            : base("DisassemblyView")
        {
            lines = new BulkObservableCollection<DisassemblyLineViewModel>();
            addressToLineMap = new Dictionary<int, DisassemblyLineViewModel>();
        }

        private DisassemblyLineViewModel GetLineByAddress(int address)
        {
            return addressToLineMap[address];
        }

        private void DebuggerService_StoryOpened(object sender, StoryEventArgs e)
        {
            var reader = e.Story.Memory.CreateReader(0);

            var blank = new DisassemblyBlankLineViewModel();

            DisassemblyLineViewModel ipLine;

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

                    var routineHeaderLine = new DisassemblyRoutineHeaderLineViewModel(routine);
                    lines.Add(routineHeaderLine);
                    addressToLineMap.Add(routine.Address, routineHeaderLine);

                    lines.Add(blank);

                    foreach (var i in routine.Instructions)
                    {
                        var instructionLine = new DisassemblyInstructionLineViewModel(i);
                        lines.Add(instructionLine);
                        addressToLineMap.Add(i.Address, instructionLine);
                    }
                }

                ipLine = GetLineByAddress(e.Story.Processor.PC);
                ipLine.HasIP = true;
            }
            finally
            {
                lines.EndBulkOperation();
            }

            BringLineIntoView(ipLine);

            e.Story.Processor.Stepped += Processor_Stepped;
            e.Story.Processor.EnterFrame += Processor_EnterFrame;
            e.Story.Processor.ExitFrame += Processor_ExitFrame;
        }

        private void BringLineIntoView(DisassemblyLineViewModel line)
        {
            var lines = this.View.FindName<ItemsControl>("lines");
            lines.BringIntoView(line);
        }

        private void Processor_Stepped(object sender, ProcessorSteppedEventArgs e)
        {
            var oldLine = GetLineByAddress(e.OldPC);
            oldLine.HasIP = false;

            var newLine = GetLineByAddress(e.NewPC);
            newLine.HasIP = true;

            BringLineIntoView(newLine);
        }

        private void Processor_EnterFrame(object sender, StackFrameEventArgs e)
        {
            var returnLine = GetLineByAddress(e.NewFrame.ReturnAddress);
            returnLine.IsNextInstruction = true;
            returnLine.ToolTip = new CallToolTip(e.NewFrame);
        }

        private void Processor_ExitFrame(object sender, StackFrameEventArgs e)
        {
            var returnLine = GetLineByAddress(e.OldFrame.ReturnAddress);
            returnLine.IsNextInstruction = false;
            returnLine.ToolTip = false;
        }

        private void DebuggerService_StoryClosed(object sender, StoryEventArgs e)
        {
            lines.Clear();
            addressToLineMap.Clear();
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.StoppedAtError)
            {
                var line = GetLineByAddress(DebuggerService.Story.Processor.ExecutingInstruction.Address);
                line.State = DisassemblyLineState.Blocked;
                line.ToolTip = new ExceptionToolTip(DebuggerService.CurrentException);
            }
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            DebuggerService.StateChanged += DebuggerService_StateChanged;

            var typeface = new Typeface(this.View.FontFamily, this.View.FontStyle, this.View.FontWeight, this.View.FontStretch);

            var addressText = new FormattedText("  000000: ", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, this.View.FontSize, this.View.Foreground);
            this.View.Resources["addressWidth"] = new GridLength(addressText.WidthIncludingTrailingWhitespace);

            var opcodeName = new FormattedText("check_arg_count  ", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, this.View.FontSize, this.View.Foreground);
            this.View.Resources["opcodeWidth"] = new GridLength(opcodeName.WidthIncludingTrailingWhitespace);

        }

        public BulkObservableCollection<DisassemblyLineViewModel> Lines
        {
            get { return lines; }
        }
    }
}
