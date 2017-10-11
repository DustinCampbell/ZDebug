using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Routines;
using ZDebug.UI.Collections;
using ZDebug.UI.Controls;
using ZDebug.UI.Extensions;
using ZDebug.UI.Services;

namespace ZDebug.UI.ViewModel
{
    [Export, Shared]
    internal sealed class DisassemblyViewModel : ViewModelWithViewBase<UserControl>
    {
        private struct AddressAndIndex
        {
            public readonly int Address;
            public readonly int Index;

            public AddressAndIndex(int address, int index)
            {
                this.Address = address;
                this.Index = index;
            }
        }

        private readonly StoryService storyService;
        private readonly DebuggerService debuggerService;
        private readonly BreakpointService breakpointService;
        private readonly RoutineService routineService;
        private readonly NavigationService navigationService;
        private readonly EditRoutineNameDialogViewModel editRoutineNameDialogViewModel;

        private readonly BulkObservableCollection<DisassemblyLineViewModel> lines;
        private readonly IntegerMap<DisassemblyLineViewModel> addressToLineMap;
        private readonly List<AddressAndIndex> routineAddressAndIndexList;

        private DisassemblyLineViewModel inputLine;

        [ImportingConstructor]
        public DisassemblyViewModel(
            StoryService storyService,
            DebuggerService debuggerService,
            BreakpointService breakpointService,
            RoutineService routineService,
            NavigationService navigationService,
            EditRoutineNameDialogViewModel editRoutineNameDialogViewModel)
            : base("DisassemblyView")
        {
            this.storyService = storyService;

            this.debuggerService = debuggerService;
            this.debuggerService.MachineCreated += DebuggerService_MachineCreated;
            this.debuggerService.MachineDestroyed += DebuggerService_MachineDestroyed;
            this.debuggerService.StateChanged += DebuggerService_StateChanged;
            this.debuggerService.Stepped += DebuggerService_Stepped;

            this.breakpointService = breakpointService;
            this.breakpointService.Added += BreakpointService_Added;
            this.breakpointService.Removed += BreakpointService_Removed;

            this.routineService = routineService;
            this.routineService.RoutineNameChanged += RoutineService_RoutineNameChanged;

            this.navigationService = navigationService;
            this.navigationService.NavigationRequested += NavigationService_NavigationRequested;

            this.editRoutineNameDialogViewModel = editRoutineNameDialogViewModel;

            lines = new BulkObservableCollection<DisassemblyLineViewModel>();
            addressToLineMap = new IntegerMap<DisassemblyLineViewModel>();
            routineAddressAndIndexList = new List<AddressAndIndex>();

            this.EditNameCommand = RegisterCommand<int>(
                text: "EditName",
                name: "Edit Name",
                executed: EditNameExecuted,
                canExecute: CanEditNameExecute);
        }

        private bool CanEditNameExecute(int address)
        {
            return true;
        }

        private void EditNameExecuted(int address)
        {
            var routineViewModel = GetLineByAddress(address) as DisassemblyRoutineHeaderLineViewModel;
            if (routineViewModel == null)
            {
                // TODO: Show error
                return;
            }

            editRoutineNameDialogViewModel.Name = routineViewModel.Name;
            if (editRoutineNameDialogViewModel.ShowDialog(owner: App.Current.MainWindow) == true)
            {
                routineService.SetRoutineName(address, editRoutineNameDialogViewModel.Name);
            }
        }

        public ICommand EditNameCommand { get; private set; }

        private DisassemblyLineViewModel GetLineByAddress(int address)
        {
            DisassemblyLineViewModel result;
            if (addressToLineMap.TryGetValue(address, out result))
            {
                return result;
            }

            return null;
        }

        private void DebuggerService_MachineCreated(object sender, MachineCreatedEventArgs e)
        {
            var reader = new MemoryReader(storyService.Story.Memory, 0);

            DisassemblyLineViewModel ipLine;

            lines.BeginBulkOperation();
            try
            {
                var routineTable = routineService.RoutineTable;

                for (int rIndex = 0; rIndex < routineTable.Count; rIndex++)
                {
                    var routine = routineTable[rIndex];

                    if (rIndex > 0)
                    {
                        var lastRoutine = routineTable[rIndex - 1];
                        if (lastRoutine.Address + lastRoutine.Length < routine.Address)
                        {
                            var addressGapLine = new DisassemblyAddressGapLineViewModel(lastRoutine, routine)
                            {
                                ShowBlankBefore = true
                            };

                            lines.Add(addressGapLine);
                        }
                    }

                    var routineHeaderLine = new DisassemblyRoutineHeaderLineViewModel(routine)
                    {
                        ShowBlankBefore = rIndex > 0,
                        ShowBlankAfter = true
                    };

                    routineAddressAndIndexList.Add(new AddressAndIndex(routineHeaderLine.Address, lines.Count));
                    lines.Add(routineHeaderLine);
                    addressToLineMap.Add(routine.Address, routineHeaderLine);

                    var instructions = routine.Instructions;
                    var lastIndex = instructions.Length - 1;
                    for (int i = 0; i <= lastIndex; i++)
                    {
                        var instruction = instructions[i];
                        var instructionLine = new DisassemblyInstructionLineViewModel(instruction, i == lastIndex);

                        if (breakpointService.Exists(instruction.Address))
                        {
                            instructionLine.HasBreakpoint = true;
                        }

                        lines.Add(instructionLine);
                        addressToLineMap.Add(instruction.Address, instructionLine);
                    }
                }

                ipLine = GetLineByAddress(debuggerService.Machine.PC);
                ipLine.HasIP = true;
            }
            finally
            {
                lines.EndBulkOperation();
            }

            BringLineIntoView(ipLine);

            routineService.RoutineTable.RoutineAdded += RoutineTable_RoutineAdded;
        }

        private void BringLineIntoView(DisassemblyLineViewModel line)
        {
            var lines = this.View.FindName<ItemsControl>("lines");
            lines.BringIntoView(line);
        }

        private void DebuggerService_Stepped(object sender, SteppedEventArgs e)
        {
            if (debuggerService.State == DebuggerState.Running)
            {
                return;
            }

            var oldLine = GetLineByAddress(e.OldPC);
            oldLine.HasIP = false;

            if (debuggerService.State == DebuggerState.AwaitingInput ||
                debuggerService.State == DebuggerState.Done)
            {
                return;
            }

            var newLine = GetLineByAddress(e.NewPC);
            newLine.HasIP = true;

            BringLineIntoView(newLine);
        }

        private void RoutineTable_RoutineAdded(object sender, ZRoutineAddedEventArgs e)
        {
            lines.BeginBulkOperation();
            try
            {
                // Find routine header line that would follow this routine
                int nextRoutineIndex = -1;
                int insertionPoint = -1;
                for (int i = 0; i < routineAddressAndIndexList.Count; i++)
                {
                    var addressAndIndex = routineAddressAndIndexList[i];
                    if (addressAndIndex.Address > e.Routine.Address)
                    {
                        nextRoutineIndex = i;
                        insertionPoint = addressAndIndex.Index;
                        break;
                    }
                }

                // If no routine header found, insert at the end of the list.
                if (nextRoutineIndex == -1)
                {
                    insertionPoint = lines.Count;
                }

                var count = 0;

                // Is previous line an address gap? If so, we need to either remove it or update it in place.
                if (insertionPoint > 0)
                {
                    var addressGap = lines[insertionPoint - 1] as DisassemblyAddressGapLineViewModel;
                    if (addressGap != null)
                    {
                        var priorRoutine = addressGap.Start;
                        var nextRoutine = addressGap.End;

                        if (addressGap.StartAddress == e.Routine.Address)
                        {
                            // If the address gap starts at this routine, we need to remove it.
                            lines.RemoveAt(--insertionPoint);
                            count--;
                        }
                        else if (addressGap.StartAddress < e.Routine.Address)
                        {
                            // If the address gap starts before this routine, we need to update it in place.
                            lines[insertionPoint - 1] = new DisassemblyAddressGapLineViewModel(priorRoutine, e.Routine)
                            {
                                ShowBlankBefore = true
                            };
                        }

                        if (nextRoutine.Address > e.Routine.Address + e.Routine.Length - 1)
                        {
                            // If there is a gap between this routine and the next one, we need to insert an address gap.
                            var newAddressGap = new DisassemblyAddressGapLineViewModel(e.Routine, nextRoutine)
                            {
                                ShowBlankBefore = true
                            };

                            lines.Insert(insertionPoint, newAddressGap);
                            count++;
                        }
                    }
                }

                var instructions = e.Routine.Instructions;
                var lastIndex = instructions.Length - 1;
                for (int i = lastIndex; i >= 0; i--)
                {
                    var instruction = instructions[i];
                    var instructionLine = new DisassemblyInstructionLineViewModel(instruction, i == lastIndex);

                    if (breakpointService.Exists(instruction.Address))
                    {
                        instructionLine.HasBreakpoint = true;
                    }

                    lines.Insert(insertionPoint, instructionLine);
                    count++;
                    addressToLineMap.Add(instruction.Address, instructionLine);
                }

                var routineHeaderLine = new DisassemblyRoutineHeaderLineViewModel(e.Routine)
                {
                    ShowBlankBefore = insertionPoint > 0,
                    ShowBlankAfter = true
                };

                lines.Insert(insertionPoint, routineHeaderLine);
                count++;
                addressToLineMap.Add(e.Routine.Address, routineHeaderLine);

                if (nextRoutineIndex >= 0)
                {
                    // fix up routine indeces...
                    for (int i = nextRoutineIndex; i < routineAddressAndIndexList.Count; i++)
                    {
                        var addressAndIndex = routineAddressAndIndexList[i];
                        routineAddressAndIndexList[i] = new AddressAndIndex(addressAndIndex.Address, addressAndIndex.Index + count);
                    }

                    routineAddressAndIndexList.Insert(nextRoutineIndex, new AddressAndIndex(e.Routine.Address, insertionPoint));
                }
                else
                {
                    routineAddressAndIndexList.Add(new AddressAndIndex(e.Routine.Address, insertionPoint));
                }
            }
            finally
            {
                lines.EndBulkOperation();
            }
        }

        private void DebuggerService_MachineDestroyed(object sender, MachineDestroyedEventArgs e)
        {
            lines.Clear();
            addressToLineMap.Clear();
            routineAddressAndIndexList.Clear();
        }

        private void DebuggerService_StateChanged(object sender, DebuggerStateChangedEventArgs e)
        {
            if (e.OldState == DebuggerState.AwaitingInput)
            {
                inputLine.State = DisassemblyLineState.None;
                inputLine = null;
            }

            if (e.NewState == DebuggerState.Unavailable)
            {
                return;
            }

            if (e.NewState == DebuggerState.Running)
            {
                var line = GetLineByAddress(debuggerService.Machine.PC);
                line.HasIP = false;
            }
            else if (e.NewState == DebuggerState.StoppedAtError)
            {
                var line = GetLineByAddress(debuggerService.Machine.ExecutingAddress);
                line.State = DisassemblyLineState.Blocked;
                line.ToolTip = new ExceptionToolTip(debuggerService.CurrentException);
                BringLineIntoView(line);
            }
            else if (e.NewState == DebuggerState.Done)
            {
                var line = GetLineByAddress(debuggerService.Machine.ExecutingAddress);
                line.State = DisassemblyLineState.Stopped;
                BringLineIntoView(line);
            }
            else if (e.NewState == DebuggerState.AwaitingInput)
            {
                inputLine = GetLineByAddress(debuggerService.Machine.ExecutingAddress);
                inputLine.State = DisassemblyLineState.Paused;
                BringLineIntoView(inputLine);
            }
            else if (e.NewState == DebuggerState.Stopped &&
                (e.OldState == DebuggerState.Running || e.OldState == DebuggerState.AwaitingInput))
            {
                var line = GetLineByAddress(debuggerService.Machine.PC);
                line.HasIP = true;
                BringLineIntoView(line);
            }
        }

        private void BreakpointService_Removed(object sender, BreakpointEventArgs e)
        {
            var bpLine = GetLineByAddress(e.Address) as DisassemblyInstructionLineViewModel;
            if (bpLine != null)
            {
                bpLine.HasBreakpoint = false;
            }
        }

        private void BreakpointService_Added(object sender, BreakpointEventArgs e)
        {
            var bpLine = GetLineByAddress(e.Address) as DisassemblyInstructionLineViewModel;
            if (bpLine != null)
            {
                bpLine.HasBreakpoint = true;
            }
        }

        private void NavigationService_NavigationRequested(object sender, NavigationRequestedEventArgs e)
        {
            var line = GetLineByAddress(e.Address);
            if (line != null)
            {
                BringLineIntoView(line);
            }
            else
            {
                // TODO: Show error message
            }
        }

        private void RoutineService_RoutineNameChanged(object sender, RoutineNameChangedEventArgs e)
        {
            var line = GetLineByAddress(e.Routine.Address) as DisassemblyRoutineHeaderLineViewModel;
            if (line != null)
            {
                line.NameUpdated();
            }
            else
            {
                // TODO: Show error message
            }
        }

        protected override void ViewCreated(UserControl view)
        {
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
