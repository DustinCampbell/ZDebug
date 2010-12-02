using System;
using System.IO;
using ZDebug.Core;

namespace ZDebug.UI.Services
{
    internal static class DebuggerService
    {
        private static DebuggerState state;
        private static Story story;
        private static string fileName;

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

        public static void CloseStory()
        {
            if (story == null)
            {
                return;
            }

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

            var handler = StoryOpened;
            if (handler != null)
            {
                handler(null, new StoryEventArgs(story));
            }

            ChangeState(DebuggerState.Stopped);

            return story;
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

        public static event EventHandler<DebuggerStateChangedEventArgs> StateChanged;

        public static event EventHandler<StoryEventArgs> StoryClosed;
        public static event EventHandler<StoryEventArgs> StoryOpened;
    }
}
