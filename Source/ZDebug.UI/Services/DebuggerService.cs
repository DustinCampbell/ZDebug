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
        private static DebuggerState state;
        private static bool stopping;
        private static bool hasStepped;

        private static IInterpreter interpreter;
        private static StoryService storyService;
        private static InterpretedZMachine processor;
        private static ZRoutineTable routineTable;
        private static InstructionReader reader;
        private static Instruction currentInstruction;
        private static string fileName;
        private static Exception currentException;
        private readonly static SortedSet<int> breakpoints = new SortedSet<int>();
        private readonly static List<string> gameScript = new List<string>();
        private static int gameScriptCommandIndex;

        private static DebuggerState priorState;

        static DebuggerService()
        {
            storyService = new StoryService();
            storyService.StoryOpened += storyService_StoryOpened;
            storyService.StoryClosing += storyService_StoryClosing;
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
                foreach (var bpElem in bpsElem.Elements("breakpoint"))
                {
                    var addAttr = bpElem.Attribute("address");
                    breakpoints.Add((int)addAttr);
                }
            }

            var scriptElem = xml.Element("gamescript");
            if (scriptElem != null)
            {
                foreach (var commandElem in scriptElem.Elements("command"))
                {
                    gameScript.Add(commandElem.Value);
                }
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
                        breakpoints.Select(b => new XElement("breakpoint", new XAttribute("address", b)))),
                    new XElement("gamescript",
                        gameScript.Select(c => new XElement("command", c))),
                    new XElement("knownroutines",
                        routineTable.Select(r => new XElement("routine",
                            new XAttribute("address", r.Address),
                            new XAttribute("name", r.Name)))));

            GameStorage.SaveStorySettings(story, xml);
        }

        private static void storyService_StoryClosing(object sender, StoryClosingEventArgs e)
        {
            SaveSettings(e.Story);

            interpreter = null;
            processor = null;
            routineTable = null;
            reader = null;
            currentInstruction = null;
            fileName = null;
            hasStepped = false;

            breakpoints.Clear();
            gameScript.Clear();

            var handler = StoryClosed;
            if (handler != null)
            {
                handler(null, new StoryEventArgs(e.Story));
            }

            ChangeState(DebuggerState.Unavailable);
        }

        private static void storyService_StoryOpened(object sender, StoryOpenedEventArgs e)
        {
            interpreter = new Interpreter();
            e.Story.RegisterInterpreter(interpreter);
            var cache = new InstructionCache();
            processor = new InterpretedZMachine(e.Story);
            routineTable = new ZRoutineTable(e.Story, cache);
            reader = new InstructionReader(processor.PC, e.Story.Memory, cache);

            LoadSettings(e.Story);

            gameScriptCommandIndex = gameScript.Count != 0 ? 0 : -1;
            processor.SetRandomSeed(42);

            processor.Quit += Processor_Quit;

            var handler = StoryOpened;
            if (handler != null)
            {
                handler(null, new StoryEventArgs(e.Story));
            }

            ChangeState(DebuggerState.Stopped);
        }

        public static void CloseStory()
        {
            storyService.CloseStory();
        }

        public static Story OpenStory(string fileName)
        {
            DebuggerService.fileName = fileName;
            return storyService.OpenStory(fileName);
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

        public static void AddBreakpoint(int address)
        {
            breakpoints.Add(address);

            var handler = BreakpointAdded;
            if (handler != null)
            {
                handler(null, new BreakpointEventArgs(address));
            }
        }

        public static void RemoveBreakpoint(int address)
        {
            breakpoints.Remove(address);

            var handler = BreakpointRemoved;
            if (handler != null)
            {
                handler(null, new BreakpointEventArgs(address));
            }
        }

        public static void ToggleBreakpoint(int address)
        {
            if (breakpoints.Contains(address))
            {
                RemoveBreakpoint(address);
            }
            else
            {
                AddBreakpoint(address);
            }
        }

        public static bool BreakpointExists(int address)
        {
            return breakpoints.Contains(address);
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

                    if (state == DebuggerState.Running && breakpoints.Contains(newPC))
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
            string fileName = DebuggerService.fileName;
            CloseStory();
            OpenStory(fileName);
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
                if (breakpoints.Contains(processor.PC))
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

        public static Story Story
        {
            get { return storyService.Story; }
        }

        public static bool HasStory
        {
            get { return storyService.IsStoryOpen; }
        }

        public static InterpretedZMachine Processor
        {
            get { return processor; }
        }

        public static GameInfo GameInfo
        {
            get { return storyService.GameInfo; }
        }

        public static bool HasGameInfo
        {
            get { return storyService.HasGameInfo; }
        }

        public static ZRoutineTable RoutineTable
        {
            get { return routineTable; }
        }

        public static bool HasFileName
        {
            get { return !string.IsNullOrWhiteSpace(fileName); }
        }

        public static string FileName
        {
            get { return fileName; }
        }

        public static Exception CurrentException
        {
            get { return currentException; }
        }

        public static IEnumerable<int> Breakpoints
        {
            get
            {
                foreach (var address in breakpoints)
                {
                    yield return address;
                }
            }
        }

        public static void SetGameScriptCommands(IEnumerable<string> commands)
        {
            gameScript.Clear();
            gameScript.AddRange(commands);
        }

        public static string[] GetGameScriptCommands()
        {
            return gameScript.ToArray();
        }

        public static bool HasGameScriptCommand()
        {
            return gameScriptCommandIndex >= 0 && gameScriptCommandIndex < gameScript.Count;
        }

        public static string GetNextGameScriptCommand()
        {
            if (gameScriptCommandIndex < 0 || gameScriptCommandIndex >= gameScript.Count)
            {
                throw new InvalidOperationException();
            }

            return gameScript[gameScriptCommandIndex++];
        }

        public static int GameScriptCommandCount
        {
            get { return gameScript.Count; }
        }

        public static event EventHandler<DebuggerStateChangedEventArgs> StateChanged;

        public static event EventHandler<StoryEventArgs> StoryClosed;
        public static event EventHandler<StoryEventArgs> StoryOpened;

        public static event EventHandler<BreakpointEventArgs> BreakpointAdded;
        public static event EventHandler<BreakpointEventArgs> BreakpointRemoved;

        public static event EventHandler<ProcessorSteppingEventArgs> ProcessorStepping;
        public static event EventHandler<ProcessorSteppedEventArgs> ProcessorStepped;

        public static event EventHandler<NavigationRequestedEventArgs> NavigationRequested;

        public static event EventHandler<RoutineNameChangedEventArgs> RoutineNameChanged;
    }
}
