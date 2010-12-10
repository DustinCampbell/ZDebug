using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
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

            DisassemblyLineViewModel ipLine;

            lines.BeginBulkOperation();
            try
            {
                var routineTable = e.Story.RoutineTable;

                for (int rIndex = 0; rIndex < routineTable.Count; rIndex++)
                {
                    if (rIndex > 0)
                    {
                        lines.Add(DisassemblyBlankLineViewModel.Instance);
                    }

                    var routine = routineTable[rIndex];

                    var routineHeaderLine = new DisassemblyRoutineHeaderLineViewModel(routine);
                    lines.Add(routineHeaderLine);
                    addressToLineMap.Add(routine.Address, routineHeaderLine);

                    lines.Add(DisassemblyBlankLineViewModel.Instance);

                    foreach (var i in routine.Instructions)
                    {
                        var instructionLine = new DisassemblyInstructionLineViewModel(i);
                        if (DebuggerService.BreakpointExists(i.Address))
                        {
                            instructionLine.HasBreakpoint = true;
                        }
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
            e.Story.RoutineTable.RoutineAdded += RoutineTable_RoutineAdded;
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

            if (DebuggerService.State == DebuggerState.Running ||
                DebuggerService.State == DebuggerState.Done)
            {
                return;
            }

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

        private void RoutineTable_RoutineAdded(object sender, RoutineAddedEventArgs e)
        {
            lines.BeginBulkOperation();
            try
            {
                // FInd routine header line that would follow this routine
                int insertionPoint = -1;
                bool atEnd = false;
                for (int i = 0; i < lines.Count; i++)
                {
                    var headerLine = lines[i] as DisassemblyRoutineHeaderLineViewModel;
                    if (headerLine == null)
                    {
                        continue;
                    }

                    if (headerLine.Address > e.Routine.Address)
                    {
                        insertionPoint = i;
                        break;
                    }
                }

                // If no routine header found, insert at the end of the list.
                if (insertionPoint == -1)
                {
                    insertionPoint = lines.Count;
                    atEnd = true;
                }

                var routineHeaderLine = new DisassemblyRoutineHeaderLineViewModel(e.Routine);
                lines.Insert(insertionPoint++, routineHeaderLine);
                addressToLineMap.Add(e.Routine.Address, routineHeaderLine);

                lines.Add(DisassemblyBlankLineViewModel.Instance);

                foreach (var i in e.Routine.Instructions)
                {
                    var instructionLine = new DisassemblyInstructionLineViewModel(i);
                    if (DebuggerService.BreakpointExists(i.Address))
                    {
                        instructionLine.HasBreakpoint = true;
                    }
                    lines.Insert(insertionPoint++, instructionLine);
                    addressToLineMap.Add(i.Address, instructionLine);
                }

                if (!atEnd)
                {
                    lines.Insert(insertionPoint, DisassemblyBlankLineViewModel.Instance);
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
            addressToLineMap.Clear();
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.NewState == DebuggerState.StoppedAtError)
            {
                var line = GetLineByAddress(DebuggerService.Story.Processor.ExecutingInstruction.Address);
                line.State = DisassemblyLineState.Blocked;
                line.ToolTip = new ExceptionToolTip(DebuggerService.CurrentException);
                BringLineIntoView(line);
            }
            else if (e.OldState == DebuggerState.Running && e.NewState == DebuggerState.Stopped)
            {
                var line = GetLineByAddress(DebuggerService.Story.Processor.PC);
                line.HasIP = true;
                BringLineIntoView(line);
            }
            else if (e.NewState == DebuggerState.Done)
            {
                var line = GetLineByAddress(DebuggerService.Story.Processor.ExecutingInstruction.Address);
                line.State = DisassemblyLineState.Stopped;
                BringLineIntoView(line);
            }
        }

        private void DebuggerService_BreakpointRemoved(object sender, BreakpointEventArgs e)
        {
            var bpLine = GetLineByAddress(e.Address) as DisassemblyInstructionLineViewModel;
            if (bpLine != null)
            {
                bpLine.HasBreakpoint = false;
            }
        }

        private void DebuggerService_BreakpointAdded(object sender, BreakpointEventArgs e)
        {
            var bpLine = GetLineByAddress(e.Address) as DisassemblyInstructionLineViewModel;
            if (bpLine != null)
            {
                bpLine.HasBreakpoint = true;
            }
        }

        protected internal override void Initialize()
        {
            DebuggerService.StoryOpened += DebuggerService_StoryOpened;
            DebuggerService.StoryClosed += DebuggerService_StoryClosed;

            DebuggerService.StateChanged += DebuggerService_StateChanged;

            DebuggerService.BreakpointAdded += DebuggerService_BreakpointAdded;
            DebuggerService.BreakpointRemoved += DebuggerService_BreakpointRemoved;

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
