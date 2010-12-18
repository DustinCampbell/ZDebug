using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core;
using ZDebug.Core.Blorb;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.Services
{
    internal static class DebuggerService
    {
        private static DebuggerState state;
        private static Story story;
        private static GameInfo gameInfo;
        private static string fileName;
        private static Exception currentException;
        private static SortedSet<int> breakpoints = new SortedSet<int>();

        private static DebuggerState priorState;

        private static int markId = 1000;

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
                        breakpoints.Select(b => new XElement("breakpoint", new XAttribute("address", b)))));

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
            fileName = null;

            breakpoints.Clear();

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

            DebuggerService.fileName = fileName;

            LoadSettings(story);

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

            var processor = story.Processor;

            try
            {
                while (state == DebuggerState.Running)
                {
                    var newPC = processor.Step();

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
                story.Processor.Step();
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

        public static event EventHandler<DebuggerStateChangedEventArgs> StateChanged;

        public static event EventHandler<StoryEventArgs> StoryClosed;
        public static event EventHandler<StoryEventArgs> StoryOpened;

        public static event EventHandler<BreakpointEventArgs> BreakpointAdded;
        public static event EventHandler<BreakpointEventArgs> BreakpointRemoved;
    }
}
