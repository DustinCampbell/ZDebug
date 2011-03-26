using System;
using System.ComponentModel.Composition;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.Services
{
    [Export]
    internal class DebuggerService : IService
    {
        private readonly StoryService storyService;
        private readonly BreakpointService breakpointService;
        private readonly RoutineService routineService;

        private DebuggerState state;
        private bool stopping;
        private bool hasStepped;

        private InterpretedZMachine machine;
        private InstructionReader reader;
        private Instruction currentInstruction;
        private Exception currentException;

        private DebuggerState priorState;

        [ImportingConstructor]
        private DebuggerService(
            StoryService storyService,
            BreakpointService breakpointService,
            RoutineService routineService)
        {
            this.storyService = storyService;
            this.storyService.StoryOpened += StoryService_StoryOpened;
            this.storyService.StoryClosing += StoryService_StoryClosing;

            this.breakpointService = breakpointService;
            this.routineService = routineService;
        }

        private void ChangeState(DebuggerState newState)
        {
            DebuggerState oldState = state;
            state = newState;

            var handler = StateChanged;
            if (handler != null)
            {
                handler(null, new DebuggerStateChangedEventArgs(oldState, newState));
            }

            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Performs a single processor step.
        /// </summary>
        private int Step()
        {
            reader.Address = machine.PC;
            currentInstruction = reader.NextInstruction();

            var oldPC = machine.PC;
            OnStepping(oldPC);

            var newPC = machine.Step();

            // If the instruction just executed was a call, we should add the address to the
            // routine table. The address is packed inside the first operand value. Note that
            // we need to do this prior to calling firing the ProcessorStepped event to ensure
            // that the disassembly view gets updated with the new routine before attempting
            // to set the new IP.
            if (currentInstruction.Opcode.IsCall)
            {
                var callOpValue = machine.GetOperandValue(0);
                var callAddress = storyService.Story.UnpackRoutineAddress(callOpValue);
                if (callAddress != 0)
                {
                    routineService.Add(callAddress);
                }
            }

            OnStepped(oldPC, newPC);

            hasStepped = true;

            return newPC;
        }

        private void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            machine = null;
            reader = null;
            currentInstruction = null;
            hasStepped = false;

            ChangeState(DebuggerState.Unavailable);

            var handler = MachineDestroyed;
            if (handler != null)
            {
                handler(this, new MachineDestroyedEventArgs());
            }
        }

        private void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            e.Story.RegisterInterpreter(new Interpreter());
            machine = new InterpretedZMachine(e.Story);
            reader = new InstructionReader(machine.PC, e.Story.Memory);

            machine.SetRandomSeed(42);
            machine.Quit += Machine_Quit;

            ChangeState(DebuggerState.Stopped);

            var handler = MachineCreated;
            if (handler != null)
            {
                handler(this, new MachineCreatedEventArgs());
            }
        }

        private void Machine_Quit(object sender, EventArgs e)
        {
            ChangeState(DebuggerState.Done);
        }

        private void OnStepping(int oldPC)
        {
            var handler = Stepping;
            if (handler != null)
            {
                handler(this, new SteppingEventArgs(oldPC));
            }
        }

        private void OnStepped(int oldPC, int newPC)
        {
            var handler = Stepped;
            if (handler != null)
            {
                handler(this, new SteppedEventArgs(oldPC, newPC));
            }
        }

        public bool CanStartDebugging
        {
            get
            {
                return state == DebuggerState.Stopped;
            }
        }

        private void RunModePump()
        {
            try
            {
                int count = 0;
                while (state == DebuggerState.Running && count < 25000)
                {
                    int newPC = Step();

                    if (stopping)
                    {
                        stopping = false;
                        ChangeState(DebuggerState.Stopped);
                    }

                    if (state == DebuggerState.Running && breakpointService.Exists(newPC))
                    {
                        ChangeState(DebuggerState.Stopped);
                    }

                    count++;
                }

                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(RunModePump), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                currentException = ex;
                ChangeState(DebuggerState.StoppedAtError);
            }
        }

        public void StartDebugging()
        {
            ChangeState(DebuggerState.Running);

            Application.Current.Dispatcher.BeginInvoke(new Action(RunModePump), DispatcherPriority.Background);
        }

        public bool CanStopDebugging
        {
            get
            {
                return state == DebuggerState.Running;
            }
        }

        public void StopDebugging()
        {
            stopping = true;
        }

        public bool CanStepNext
        {
            get { return state == DebuggerState.Stopped; }
        }

        public void StepNext()
        {
            try
            {
                int newPC = Step();
            }
            catch (Exception ex)
            {
                currentException = ex;
                ChangeState(DebuggerState.StoppedAtError);
            }
        }

        public bool CanResetSession
        {
            get
            {
                return state != DebuggerState.Running
                    && state != DebuggerState.Unavailable && hasStepped;
            }
        }

        public void ResetSession()
        {
            string fileName = storyService.FileName;
            storyService.CloseStory();
            storyService.OpenStory(fileName);
        }

        public void BeginAwaitingInput()
        {
            if (state == DebuggerState.AwaitingInput)
            {
                throw new InvalidOperationException("Already awaiting input");
            }

            priorState = state;
            ChangeState(DebuggerState.AwaitingInput);
        }

        public void EndAwaitingInput()
        {
            if (state != DebuggerState.AwaitingInput)
            {
                throw new InvalidOperationException("Not awaiting input");
            }

            if (priorState == DebuggerState.Running)
            {
                if (breakpointService.Exists(machine.PC))
                {
                    ChangeState(DebuggerState.Stopped);
                }
                else
                {
                    StartDebugging();
                }
            }
            else
            {
                ChangeState(priorState);
            }
        }

        public DebuggerState State
        {
            get { return state; }
        }

        public InterpretedZMachine Machine
        {
            get { return machine; }
        }

        public Exception CurrentException
        {
            get { return currentException; }
        }

        public event EventHandler<MachineCreatedEventArgs> MachineCreated;
        public event EventHandler<MachineDestroyedEventArgs> MachineDestroyed;

        public event EventHandler<DebuggerStateChangedEventArgs> StateChanged;

        public event EventHandler<SteppingEventArgs> Stepping;
        public event EventHandler<SteppedEventArgs> Stepped;
    }
}
