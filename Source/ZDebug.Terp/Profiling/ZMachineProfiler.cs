using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ZDebug.Compiler.Profiling;
using ZDebug.Terp.Utilities;

namespace ZDebug.Terp.Profiling
{
    public partial class ZMachineProfiler : IZMachineProfiler
    {
        private readonly List<RoutineCompilationStatistics> allStatistics;
        private readonly Dictionary<int, Routine> routines;
        private readonly List<Call> calls;
        private readonly Stack<Call> callStack;
        private TimeSpan runningTime;

        private readonly Dictionary<int, Tuple<int, TimeSpan>> instructionTimings;
        private Stopwatch instructionTimer;

        private int routinesExecuted;
        private int instructionsExecuted;

        private HashSet<int> calculatedCalls;
        private int directCallCount;
        private int calculatedCallCount;

        public ZMachineProfiler()
        {
            this.allStatistics = new List<RoutineCompilationStatistics>();
            this.routines = new Dictionary<int, Routine>();
            this.calls = new List<Call>();
            this.callStack = new Stack<Call>();

            this.instructionTimings = new Dictionary<int, Tuple<int, TimeSpan>>();
            this.instructionTimer = new Stopwatch();

            this.calculatedCalls = new HashSet<int>();
        }

        void IZMachineProfiler.RoutineCompiled(RoutineCompilationStatistics statistics)
        {
            allStatistics.Add(statistics);
        }

        void IZMachineProfiler.Call(int address, bool calculated)
        {
            if (calculated)
            {
                calculatedCalls.Add(address);
                calculatedCallCount++;
            }
            else
            {
                directCallCount++;
            }
        }

        void IZMachineProfiler.EnterRoutine(int address)
        {
            routinesExecuted++;

            Routine routine;
            if (!routines.TryGetValue(address, out routine))
            {
                routine = new Routine(this, address);
                routines.Add(address, routine);
            }

            var index = calls.Count;
            var parent = callStack.Count > 0
                ? callStack.Peek().Index
                : -1;

            var recursive = callStack.TopToBottom().Any(c => c.Routine.Address == address);

            var call = new Call(this, routine, index, parent, recursive);
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
            instructionTimer.Restart();
        }

        void IZMachineProfiler.ExecutedInstruction(int address)
        {
            instructionTimer.Stop();

            Tuple<int, TimeSpan> timings;
            if (instructionTimings.TryGetValue(address, out timings))
            {
                timings = Tuple.Create(timings.Item1 + 1, timings.Item2.Add(instructionTimer.Elapsed));
            }
            else
            {
                timings = Tuple.Create(1, instructionTimer.Elapsed);
            }

            instructionTimings[address] = timings;

            instructionsExecuted++;
        }

        void IZMachineProfiler.Quit()
        {
        }

        void IZMachineProfiler.Interrupt()
        {
        }

        private Call GetCallByIndex(int index)
        {
            return calls[index];
        }

        public void Stop(TimeSpan runningTime)
        {
            this.runningTime = runningTime;
            while (callStack.Count > 0)
            {
                var call = callStack.Pop();
                call.Exit();
            }

            foreach (var routine in routines.Values)
            {
                routine.Done();
            }
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

        public IEnumerable<IRoutine> Routines
        {
            get
            {
                foreach (var routine in routines.Values)
                {
                    yield return routine;
                }
            }
        }

        public IEnumerable<Tuple<int, Tuple<int, TimeSpan>>> InstructionTimings
        {
            get
            {
                foreach (var timing in instructionTimings)
                {
                    var address = timing.Key;
                    var timings = timing.Value;
                    yield return Tuple.Create(address, timings);
                }
            }
        }

        public TimeSpan RunningTime
        {
            get
            {
                return runningTime;
            }
        }

        public int DirectCallCount
        {
            get
            {
                return directCallCount;
            }
        }

        public int CalculatedCallCount
        {
            get
            {
                return calculatedCallCount;
            }
        }
    }
}
