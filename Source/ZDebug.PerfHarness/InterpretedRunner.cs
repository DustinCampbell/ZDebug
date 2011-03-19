using System.IO;
using ZDebug.Core;
using ZDebug.Core.Blorb;
using System;
using System.Diagnostics;
using ZDebug.Core.Execution;

namespace ZDebug.PerfHarness
{
    public class InterpretedRunner : Runner
    {
        public InterpretedRunner(string storyFilePath, string scriptFilePath)
            : base(storyFilePath, scriptFilePath)
        {
        }

        private Story ReadStory(string path)
        {
            if (Path.GetExtension(path) == ".zblorb")
            {
                using (var stream = File.OpenRead(path))
                {
                    var blorb = new BlorbFile(stream);
                    return blorb.LoadStory();
                }
            }
            else
            {
                return Story.FromBytes(File.ReadAllBytes(path));
            }
        }

        public override void Run()
        {
            MarkProfile("Reading story");

            var story = ReadStory(StoryFilePath);
            var processor = new Processor(story);

            var done = false;
            Action doneAction = () => { done = true; };

            var mockScreen = new MockScreen(ScriptFilePath, doneAction);
            processor.SetRandomSeed(42);
            processor.RegisterScreen(mockScreen);

            processor.Quit += (s, e) => { done = true; };

            MarkProfile("Stepping...");

            var sw = Stopwatch.StartNew();

            try
            {
                while (!done)
                {
                    processor.Step();
                }
            }
            catch (Exception ex)
            {
                MarkProfile(string.Format("{0}: {1}", ex.GetType().FullName, ex.Message));
            }

            sw.Stop();

            MarkProfile("Done stepping");

            Console.WriteLine();
            Console.WriteLine("{0:#,#} instructions", processor.InstructionCount);
            Console.WriteLine("{0:#,#} calls", processor.CallCount);
            Console.WriteLine();
            Console.WriteLine("{0:#,0.##########} seconds", (double)sw.ElapsedTicks / (double)Stopwatch.Frequency);
            Console.WriteLine("{0:#,0.##########} seconds per instruction", ((double)sw.ElapsedTicks / (double)Stopwatch.Frequency) / (double)processor.InstructionCount);
        }
    }
}
