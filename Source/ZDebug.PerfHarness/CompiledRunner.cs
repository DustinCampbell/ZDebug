using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Compiler.Profiling;
using System.IO;
using ZDebug.Compiler;
using System.Diagnostics;

namespace ZDebug.PerfHarness
{
    public class CompiledRunner : Runner, IZMachineProfiler
    {
        public CompiledRunner(string storyFilePath, string scriptFilePath)
            : base(storyFilePath, scriptFilePath)
        {
        }

        public override void Run()
        {
            MarkProfile("Reading story");

            var bytes = File.ReadAllBytes(StoryFilePath);

            ZMachine machine = null;
            Action doneAction = () => { machine.Stop(); };

            var mockScreen = new MockScreen(ScriptFilePath, doneAction);
            machine = new ZMachine(bytes, mockScreen, profiler: this, debugging: true);
            machine.SetRandomSeed(42);

            MarkProfile("Stepping...");

            var sw = Stopwatch.StartNew();

            try
            {
                machine.Run();
            }
            catch (Exception ex)
            {
                MarkProfile(string.Format("{0}: {1}", ex.GetType().FullName, ex.Message));
            }

            sw.Stop();

            MarkProfile("Done stepping");

            //Console.WriteLine();
            //Console.WriteLine("{0:#,#} instructions", processor.InstructionCount);
            //Console.WriteLine("{0:#,#} calls", processor.CallCount);
            Console.WriteLine();
            Console.WriteLine("{0:#,0.##########} seconds", (double)sw.ElapsedTicks / (double)Stopwatch.Frequency);
            //Console.WriteLine("{0:#,0.##########} seconds per instruction", ((double)sw.ElapsedTicks / (double)Stopwatch.Frequency) / (double)story.Processor.InstructionCount);
        }

        void IZMachineProfiler.RoutineCompiled(RoutineCompilationStatistics statistics)
        {

        }

        private int indentLevel;

        private string GetIndent()
        {
            return new string(' ', indentLevel * 2);
        }

        void IZMachineProfiler.EnterRoutine(int address)
        {
            //Console.WriteLine(GetIndent() + address.ToString("x4"));
            //indentLevel++;
        }

        void IZMachineProfiler.ExitRoutine(int address)
        {
            //indentLevel--;
        }

        void IZMachineProfiler.ExecutingInstruction(int address)
        {
            //Console.WriteLine(GetIndent() + address.ToString("x4"));
        }
    }
}
