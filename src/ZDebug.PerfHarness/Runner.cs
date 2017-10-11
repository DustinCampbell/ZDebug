using System;

namespace ZDebug.PerfHarness
{
    public abstract class Runner
    {
        protected string StoryFilePath { get; private set; }
        protected string ScriptFilePath { get; private set; }

        public Runner(string storyFilePath, string scriptFilePath)
        {
            this.StoryFilePath = storyFilePath;
            this.ScriptFilePath = scriptFilePath;
        }

        protected void MarkProfile(string text)
        {
            Console.WriteLine(text);
        }

        public abstract void Run();
    }
}
