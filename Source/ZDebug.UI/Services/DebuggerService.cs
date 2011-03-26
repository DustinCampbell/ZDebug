using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using ZDebug.Core;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
using ZDebug.Core.Interpreter;
using ZDebug.Core.Routines;
using ZDebug.Debugger.Utilities;

namespace ZDebug.UI.Services
{
    internal static class DebuggerService
    {
        private static StoryService storyService;
        private static BreakpointService breakpointService;
        private static GameScriptService gameScriptService;

        private static DebuggerState state;
        private static bool stopping;
        private static bool hasStepped;

        private static IInterpreter interpreter;
        private static InterpretedZMachine processor;
        private static ZRoutineTable routineTable;
        private static InstructionReader reader;
        private static Instruction currentInstruction;
        private static Exception currentException;

        private static DebuggerState priorState;

        public static void SetServices(
            StoryService storyService,
            BreakpointService breakpointService,
            GameScriptService gameScriptService)
        {
            DebuggerService.storyService = storyService;
            DebuggerService.breakpointService = breakpointService;
            DebuggerService.gameScriptService = gameScriptService;

            storyService.StoryOpened += StoryService_StoryOpened;
            storyService.StoryClosing += StoryService_StoryClosing;
        }

        private static void ChangeState(DebuggerState newState)
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
        private static int Step()
        {
            reader.Address = processor.PC;
            currentInstruction = reader.NextInstruction();

            var oldPC = processor.PC;
            OnProcessorStepping(oldPC);

            var newPC = processor.Step();

            // If the instruction just executed was a call, we should add the address to the
            // routine table. The address is packed inside the first operand value. Note that
            // we need to do this prior to calling firing the ProcessorStepped event to ensure
            // that the disassembly view gets updated with the new routine before attempting
            // to set the new IP.
            if (currentInstruction.Opcode.IsCall)
            {
                var callOpValue = processor.GetOperandValue(0);
                var callAddress = storyService.Story.UnpackRoutineAddress(callOpValue);
                if (callAddress != 0)
                {
                    routineTable.Add(callAddress);
                }
            }

            OnProcessorStepped(oldPC, newPC);

            hasStepped = true;

            return newPC;
        }

        private static void LoadSettings(Story story)
        {
            var xml = GameStorage.RestoreStorySettings(story);

            var bpsElem = xml.Element("breakpoints");
            if (bpsElem != null)
            {
                var breakpoints = new List<int>();
                foreach (var bpElem in bpsElem.Elements("breakpoint"))
                {
                    var addAttr = bpElem.Attribute("address");
                    breakpoints.Add((int)addAttr);
                }
                breakpointService.SetBreakpoints(breakpoints);
            }

            var scriptElem = xml.Element("gamescript");
            if (scriptElem != null)
            {
                var commands = new List<string>();
                foreach (var commandElem in scriptElem.Elements("command"))
                {
                    commands.Add(commandElem.Value);
                }
                gameScriptService.SetCommands(commands);
            }

            var routinesElem = xml.Element("knownroutines");
            if (routinesElem != null)
            {
                foreach (var routineElem in routinesElem.Elements("routine"))
                {
                    var addAttr = routineElem.Attribute("address");
                    var nameAttr = routineElem.Attribute("name");

                    var address = (int)addAttr;
                    var name = nameAttr != null ? (string)nameAttr : null;

                    if (routineTable.Exists(address))
                    {
                        routineTable.GetByAddress(address).Name = name;
                    }
                    else
                    {
                        routineTable.Add(address, name);
                    }
                }
            }
        }

        private static void SaveSettings(Story story)
        {
            var xml =
                new XElement("settings",
                    new XElement("story",
                        new XAttribute("serial", story.SerialNumber),
                        new XAttribute("release", story.ReleaseNumber),
                        new XAttribute("version", story.Version)),
                    new XElement("breakpoints",
                        breakpointService.Breakpoints.Select(b => new XElement("breakpoint", new XAttribute("address", b)))),
                    new XElement("gamescript",
                        gameScriptService.Commands.Select(c => new XElement("command", c))),
                    new XElement("knownroutines",
                        routineTable.Select(r => new XElement("routine",
                            new XAttribute("address", r.Address),
                            new XAttribute("name", r.Name)))));

            GameStorage.SaveStorySettings(story, xml);
        }

        private static void StoryService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            SaveSettings(e.Story);

            interpreter = null;
            processor = null;
            routineTable = null;
            reader = null;
            currentInstruction = null;
            hasStepped = false;

            breakpointService.Clear();
            gameScriptService.Clear();

            ChangeState(DebuggerState.Unavailable);
        }

        private static void StoryService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            interpreter = new Interpreter();
            e.Story.RegisterInterpreter(interpreter);
            var cache = new InstructionCache();
            processor = new InterpretedZMachine(e.Story);
            routineTable = new ZRoutineTable(e.Story, cache);
            reader = new InstructionReader(processor.PC, e.Story.Memory, cache);

            LoadSettings(e.Story);

            processor.SetRandomSeed(42);

            processor.Quit += Processor_Quit;

            ChangeState(DebuggerState.Stopped);
        }

        private static void Processor_Quit(object sender, EventArgs e)
        {
            ChangeState(DebuggerState.Done);
        }

        private static void OnProcessorStepping(int oldPC)
        {
            var handler = ProcessorStepping;
            if (handler != null)
            {
                handler(null, new ProcessorSteppingEventArgs(oldPC));
            }
        }

        private static void OnProcessorStepped(int oldPC, int newPC)
        {
            var handler = ProcessorStepped;
            if (handler != null)
            {
                handler(null, new ProcessorSteppedEventArgs(oldPC, newPC));
            }
        }

        public static void RequestNavigation(int address)
        {
            var handler = NavigationRequested;
            if (handler != null)
            {
                handler(null, new NavigationRequestedEventArgs(address));
            }
        }

        public static void SetRoutineName(int address, string name)
        {
            var routine = routineTable.GetByAddress(address);
            if (routine.Name == name)
            {
                return;
            }

            routine.Name = name;

            var handler = RoutineNameChanged;
            if (handler != null)
            {
                handler(null, new RoutineNameChangedEventArgs(routine));
            }
        }

        public static bool CanStartDebugging
        {
            get { return state == DebuggerState.Stopped; }
        }

        private static void RunModePump()
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

        public static void StartDebugging()
        {
            ChangeState(DebuggerState.Running);

            Application.Current.Dispatcher.BeginInvoke(new Action(RunModePump), DispatcherPriority.Background);
        }

        public static bool CanStopDebugging
        {
            get { return state == DebuggerState.Running; }
        }

        public static void StopDebugging()
        {
            stopping = true;
        }

        public static bool CanStepNext
        {
            get { return state == DebuggerState.Stopped; }
        }

        public static void StepNext()
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

        public static bool CanResetSession
        {
            get { return state != DebuggerState.Running && state != DebuggerState.Unavailable && hasStepped; }
        }

        public static void ResetSession()
        {
            string fileName = storyService.FileName;
            storyService.CloseStory();
            storyService.OpenStory(fileName);
        }

        public static void BeginAwaitingInput()
        {
            if (state == DebuggerState.AwaitingInput)
            {
                throw new InvalidOperationException("Already awaiting input");
            }

            priorState = state;
            ChangeState(DebuggerState.AwaitingInput);
        }

        public static void EndAwaitingInput()
        {
            if (state != DebuggerState.AwaitingInput)
            {
                throw new InvalidOperationException("Not awaiting input");
            }

            if (priorState == DebuggerState.Running)
            {
                if (breakpointService.Exists(processor.PC))
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

        public static DebuggerState State
        {
            get { return state; }
        }

        public static InterpretedZMachine Processor
        {
            get { return processor; }
        }

        public static ZRoutineTable RoutineTable
        {
            get { return routineTable; }
        }

        public static Exception CurrentException
        {
            get { return currentException; }
        }

        public static event EventHandler<DebuggerStateChangedEventArgs> StateChanged;

        public static event EventHandler<ProcessorSteppingEventArgs> ProcessorStepping;
        public static event EventHandler<ProcessorSteppedEventArgs> ProcessorStepped;

        public static event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

        public static event EventHandler<RoutineNameChangedEventArgs> RoutineNameChanged;
    }
}
