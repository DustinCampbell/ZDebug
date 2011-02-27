using System.Collections.Generic;
using ZDebug.Compiler.Profiling;

namespace ZDebug.Terp.Profiling
{
    public class ZMachineProfiler : IZMachineProfiler
    {
        private readonly List<RoutineCompilationStatistics> allStatistics = new List<RoutineCompilationStatistics>();

        private int routinesExecuted;
        private int instructionsExecuted;

        void IZMachineProfiler.RoutineCompiled(RoutineCompilationStatistics statistics)
        {
            allStatistics.Add(statistics);
        }

        void IZMachineProfiler.EnterRoutine(int address)
        {
            routinesExecuted++;
        }

        void IZMachineProfiler.ExitRoutine(int address)
        {

        }

        void IZMachineProfiler.ExecutingInstruction(int address)
        {
            instructionsExecuted++;
        }

        public IEnumerable<RoutineCompilationStatistics> CompilationStatistics
        {
            get
            {
                foreach (var stat in allStatistics)
                {
                    yield return stat;
                }
            }
        }

        public int RoutinesCompiled
        {
            get
            {
                return allStatistics.Count;
            }
        }

        public int RoutinesExecuted
        {
            get
            {
                return routinesExecuted;
            }
        }

        public int InstructionsExecuted
        {
            get
            {
                return instructionsExecuted;
            }
        }
    }
}
