using System;
using System.Diagnostics;
using System.IO;
using Microsoft.VisualStudio.Profiler;
using ZDebug.Core;
using ZDebug.Core.Blorb;

namespace ZDebug.PerfHarness
{
    class Program
    {
        const string BRONZE = @"..\..\ZCode\bronze\bronze.z8";
        const string DREAMHOLD = @"..\..\ZCode\dreamhold\dreamhold.z8";
        const string SANDDANC = @"..\..\ZCode\sanddanc\sanddanc.z5";
        const string ROTA = @"..\..\ZCode\rota\RoTA.zblorb";
        const string ZORK1 = @"..\..\ZCode\zork1\zork1.z3";

        const string ZORK1_SCRIPT = @"..\..\ZCode\zork1\zork1_script.txt";

        static Story ReadStory(string path)
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

        static int markID = 1000;

        static void Mark(string text)
        {
            DataCollection.CommentMarkProfile(++markID, text);
            Console.WriteLine(text);
        }

        static void Main(string[] args)
        {
            string path = ZORK1;

            Mark("Reading story");

            var story = ReadStory(path);

            var processor = story.Processor;

            var done = false;
            var mockScreen = new MockScreen(ZORK1_SCRIPT);
            //var mockScreen = new MockScreen(() => { done = true; });
            processor.SetRandomSeed(42);
            processor.RegisterScreen(mockScreen);

            processor.Quit += (s, e) => { done = true; };

            Mark("Stepping...");

            var sw = Stopwatch.StartNew();

            try
            {
                while (!done)
                {
                    processor.Step();
                }
            }
            catch
            {
            }

            sw.Stop();

            Mark("Done stepping");

            DataCollection.StopProfile(ProfileLevel.Process, DataCollection.CurrentId);

            Console.WriteLine();
            Console.WriteLine("{0:#,#} instructions", processor.InstructionCount);
            Console.WriteLine("{0:#,#} calls", processor.CallCount);
            Console.WriteLine();
            Console.WriteLine("{0:#,0.##########} seconds", (double)sw.ElapsedTicks / (double)Stopwatch.Frequency);
            Console.WriteLine("{0:#,0.##########} seconds per instruction", ((double)sw.ElapsedTicks / (double)Stopwatch.Frequency) / (double)story.Processor.InstructionCount);
        }
    }
}
