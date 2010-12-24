using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core;
using ZDebug.Core.Blorb;
using ZDebug.UI.Utilities;
using ZDebug.Core.Instructions;
using ZDebug.Core.Execution;

namespace ZDebug.UI.Services
{
    internal static class DebuggerService
    {
        private static DebuggerState state;
        private static Story story;
        private static GameInfo gameInfo;
        private static RoutineTable routineTable;
        private static InstructionReader reader;
        private static Instruction currentInstruction;
        private static string fileName;
        private static Exception currentException;
        private readonly static SortedSet<int> breakpoints = new SortedSet<int>();
        private readonly static List<string> gameScript = new List<string>();
        private static int gameScriptCommandIndex;

        private static DebuggerState priorState;

        private static void ChangeState(DebuggerState newState)
        {
            var oldState = state;
            state = newState;

            var handler = StateChanged;
            if (handler != null)
            {
                handler(null, new DebuggerStateChangedEventArgs(oldState, newState));
            }
        }

        /// <summary>
        /// Performs a single processor step.
        /// </summary>
        private static int Step()
        {
            var processor = story.Processor;

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
                var callAddress = story.UnpackRoutineAddress(callOpValue);
                routineTable.Add(callAddress);
            }

            OnProcessorStepped(oldPC, newPC);

            return newPC;
        }

        private static void LoadSettings(Story story)
        {
            var xml = Storage.RestoreStorySettings(story);

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
                    routineTable.Add((int)addAttr);
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
                        routineTable.Select(r => new XElement("routine", new XAttribute("address", r.Address)))));

            Storage.SaveStorySettings(story, xml);
        }

        public static void CloseStory()
        {
            if (story == null)
            {
                return;
            }

            SaveSettings(story);

            var oldStory = story;

            story = null;
            gameInfo = null;
            routineTable = null;
            reader = null;
            currentInstruction = null;
            fileName = null;

            breakpoints.Clear();
            gameScript.Clear();

            var handler = StoryClosed;
            if (handler != null)
            {
                handler(null, new StoryEventArgs(oldStory));
            }

            ChangeState(DebuggerState.Unavailable);
        }

        public static Story OpenStory(string fileName)
        {
            CloseStory();

            if (Path.GetExtension(fileName) == ".zblorb")
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var blorb = new BlorbFile(stream);
                    gameInfo = new GameInfo(blorb);
                    DebuggerService.story = blorb.LoadStory();
                }
            }
            else
            {
                DebuggerService.story = Story.FromBytes(File.ReadAllBytes(fileName));
            }

            var cache = new InstructionCache();
            routineTable = new RoutineTable(story, cache);
            reader = new InstructionReader(story.Processor.PC, story.Memory, cache);

            DebuggerService.fileName = fileName;

            LoadSettings(story);

            gameScriptCommandIndex = gameScript.Count != 0 ? 0 : -1;
            story.Processor.SetRandomSeed(42);

            story.Processor.Quit += Processor_Quit;

            var handler = StoryOpened;
            if (handler != null)
            {
                handler(null, new StoryEventArgs(story));
            }

            ChangeState(DebuggerState.Stopped);

            return story;
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

        public static bool CanStartDebugging
        {
            get { return state == DebuggerState.Stopped; }
        }

        public static void StartDebugging()
        {
            ChangeState(DebuggerState.Running);

            try
            {
                while (state == DebuggerState.Running)
                {
                    int newPC = Step();

                    if (state == DebuggerState.Running && breakpoints.Contains(newPC))
                    {
                        ChangeState(DebuggerState.Stopped);
                    }
                }
            }
            catch (Exception ex)
            {
                currentException = ex;
                ChangeState(DebuggerState.StoppedAtError);
            }
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
                if (breakpoints.Contains(story.Processor.PC))
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
            get { return story; }
        }

        public static bool HasStory
        {
            get { return story != null; }
        }

        public static GameInfo GameInfo
        {
            get { return gameInfo; }
        }

        public static bool HasGameInfo
        {
            get { return gameInfo != null; }
        }

        public static RoutineTable RoutineTable
        {
            get { return routineTable; }
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
    }
}
