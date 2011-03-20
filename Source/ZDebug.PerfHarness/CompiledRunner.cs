using System;
using System.Diagnostics;
using System.IO;
using ZDebug.Compiler;
using ZDebug.Compiler.Profiling;
using ZDebug.Core;

namespace ZDebug.PerfHarness
{
    public class CompiledRunner : Runner, IZMachineProfiler
    {
        private Stopwatch watch;
        private int callCount;
        private int instructionCount;
        private bool profile;

        public CompiledRunner(string storyFilePath, string scriptFilePath, bool profile = false)
            : base(storyFilePath, scriptFilePath)
        {
            this.profile = profile;
        }

        public override void Run()
        {
            MarkProfile("Reading story");

            var bytes = File.ReadAllBytes(StoryFilePath);

            CompiledZMachine machine = null;
            Action doneAction = () => { machine.Stop(); };

            var mockScreen = new MockScreen(ScriptFilePath, doneAction);
            machine = new CompiledZMachine(Story.FromBytes(bytes), profile ? this : null);
            machine.RegisterScreen(mockScreen);
            machine.SetRandomSeed(42);

            MarkProfile("Running...");

            watch = Stopwatch.StartNew();

            try
            {
                machine.Run();
            }
            catch (ZMachineQuitException)
            {
                // done
            }
            catch (ZMachineInterruptedException)
            {
                // done
            }
            catch (Exception ex)
            {
                MarkProfile(string.Format("{0}: {1}", ex.GetType().FullName, ex.Message));
            }

            watch.Stop();

            MarkProfile("Done");

            if (profile)
            {
                Console.WriteLine();
                Console.WriteLine("{0:#,#} instructions", instructionCount);
                Console.WriteLine("{0:#,#} calls", callCount);
            }
            Console.WriteLine();
            Console.WriteLine("{0:#,0.##########} seconds", (double)watch.ElapsedTicks / (double)Stopwatch.Frequency);
            if (profile)
            {
                Console.WriteLine("{0:#,0.##########} seconds per instruction", ((double)watch.ElapsedTicks / (double)Stopwatch.Frequency) / (double)instructionCount);
            }
        }

        void IZMachineProfiler.RoutineCompiled(RoutineCompilationStatistics statistics)
        {
        }

        void IZMachineProfiler.Call(int address, bool calculated)
        {
        }

        void IZMachineProfiler.EnterRoutine(int address)
        {
            callCount++;
        }

        void IZMachineProfiler.ExitRoutine(int address)
        {
        }

        void IZMachineProfiler.ExecutingInstruction(int address)
        {
        }

        void IZMachineProfiler.ExecutedInstruction(int address)
        {
            instructionCount++;
        }

        void IZMachineProfiler.Quit()
        {
            watch.Stop();
        }

        void IZMachineProfiler.Interrupt()
        {
            watch.Stop();
        }
    }
}
