using System;
using System.IO;
using Microsoft.VisualStudio.Profiler;
using ZDebug.Core;
using ZDebug.Core.Blorb;

namespace ZDebug.PerfHarness
{
    class Program
    {
        static byte[] ReadStoryBytes(string path)
        {
            using (var stream = File.OpenRead(path))
            {
                var blorb = new BlorbFile(stream);
                return blorb.GetZCode();
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
            Mark("Reading " + Path.GetFileName("RoTA.zblorb") + " bytes...");

            var bytes = ReadStoryBytes("..\\..\\ZCode\\rota\\RoTA.zblorb");

            Mark("Creating story");

            var story = Story.FromBytes(bytes);

            Mark("Stepping...");

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

            Mark("Done stepping");

            DataCollection.StopProfile(ProfileLevel.Process, DataCollection.CurrentId);

            Console.WriteLine();
            Console.WriteLine("{0:#,#} instructions", story.Processor.InstructionCount);
            Console.WriteLine("{0:#,#} calls", story.Processor.CallCount);
        }
    }
}
