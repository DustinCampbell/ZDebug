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
        const string ROTA = @"..\..\ZCode\rota\RoTA.zblorb";
        const string ZORK1 = @"..\..\ZCode\zork1\zork1.z3";

        static byte[] ReadStoryBytes(string path)
        {
            if (Path.GetExtension(path) == ".zblorb")
            {
                using (var stream = File.OpenRead(path))
                {
                    var blorb = new BlorbFile(stream);
                    return blorb.GetZCode();
                }
            }
            else
            {
                return File.ReadAllBytes(path);
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
            string path = ROTA;

            Mark("Reading " + Path.GetFileName(path) + " bytes...");

            var bytes = ReadStoryBytes(path);

            Mark("Creating story");

            var story = Story.FromBytes(bytes);

            Mark("Stepping...");

            var sw = Stopwatch.StartNew();

            try
            {
                while (true)
                {
                    story.Processor.Step();
                }
            }
            catch
            {
            }

            sw.Stop();

            Mark("Done stepping");

            DataCollection.StopProfile(ProfileLevel.Process, DataCollection.CurrentId);

            Console.WriteLine();
            Console.WriteLine("{0:#,#} instructions", story.Processor.InstructionCount);
            Console.WriteLine("{0:#,#} calls", story.Processor.CallCount);
            Console.WriteLine();
            Console.WriteLine("{0:#,0.##########} seconds", (double)sw.ElapsedTicks / (double)Stopwatch.Frequency);
            Console.WriteLine("{0:#,0.##########} seconds per instruction", ((double)sw.ElapsedTicks / (double)Stopwatch.Frequency) / (double)story.Processor.InstructionCount);
        }
    }
}
