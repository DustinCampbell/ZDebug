using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Profiler;

namespace ZDebug.PerfHarness
{
    public abstract class Runner
    {
        protected string StoryFilePath { get; private set; }
        protected string ScriptFilePath { get; private set; }

        private int markID = 1000;

        public Runner(string storyFilePath, string scriptFilePath)
        {
            this.StoryFilePath = storyFilePath;
            this.ScriptFilePath = scriptFilePath;
        }

        protected void MarkProfile(string text)
        {
            DataCollection.CommentMarkProfile(++markID, text);
            Console.WriteLine(text);
        }

        public abstract void Run();
    }
}
