using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ZDebug.Core;
using ZDebug.UI.Utilities;

namespace ZDebug.UI.Services
{
    internal static class DebuggerService
    {
        private static DebuggerState state;
        private static Story story;
        private static string fileName;
        private static Exception currentException;
        private static SortedSet<int> breakpoints = new SortedSet<int>();

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
            fileName = null;

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

            var bytes = File.ReadAllBytes(fileName);
            DebuggerService.story = Story.FromBytes(bytes);
            DebuggerService.fileName = fileName;

            LoadSettings(story);

            var handler = StoryOpened;
            if (handler != null)
            {
                handler(null, new StoryEventArgs(story));
            }

            ChangeState(DebuggerState.Stopped);

            return story;
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

        public static string FileName
        {
            get { return fileName; }
        }

        public static Exception CurrentException
        {
            get { return currentException; }
        }

        public static event EventHandler<DebuggerStateChangedEventArgs> StateChanged;

        public static event EventHandler<StoryEventArgs> StoryClosed;
        public static event EventHandler<StoryEventArgs> StoryOpened;
    }
}
