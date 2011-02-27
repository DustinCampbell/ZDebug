using System.Collections.Generic;
using ZDebug.Compiler.Profiling;
using ZDebug.Core.Collections;

namespace ZDebug.Terp.Profiling
{
    public partial class ZMachineProfiler : IZMachineProfiler
    {
        private readonly List<RoutineCompilationStatistics> allStatistics;
        private readonly IntegerMap<Routine> routines;
        private readonly List<Call> calls;
        private readonly Stack<Call> callStack;

        private int routinesExecuted;
        private int instructionsExecuted;

        public ZMachineProfiler()
        {
            this.allStatistics = new List<RoutineCompilationStatistics>();
            this.routines = new IntegerMap<Routine>();
            this.calls = new List<Call>();
            this.callStack = new Stack<Call>();
        }

        void IZMachineProfiler.RoutineCompiled(RoutineCompilationStatistics statistics)
        {
            allStatistics.Add(statistics);
        }

        void IZMachineProfiler.EnterRoutine(int address)
        {
            routinesExecuted++;

            Routine routine;
            if (!routines.TryGetValue(address, out routine))
            {
                routine = new Routine(address);
                routines.Add(address, routine);
            }

            var index = calls.Count;
            var parent = callStack.Count > 0
                ? callStack.Peek().Index
                : -1;

            var call = new Call(this, routine, index, parent);
            calls.Add(call);
            callStack.Push(call);

            call.Enter();
        }

        void IZMachineProfiler.ExitRoutine(int address)
        {
            var call = callStack.Pop();
            call.Exit();
        }

        void IZMachineProfiler.ExecutingInstruction(int address)
        {
            instructionsExecuted++;
        }

        private Call GetCallByIndex(int index)
        {
            return calls[index];
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

        public ICall RootCall
        {
            get
            {
                return calls[0];
            }
        }
    }
}
