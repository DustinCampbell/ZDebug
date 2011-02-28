using System;
using System.Collections.Generic;
using System.Diagnostics;
using ZDebug.Compiler.Profiling;

namespace ZDebug.Terp.Profiling
{
    public partial class ZMachineProfiler : IZMachineProfiler
    {
        private readonly List<RoutineCompilationStatistics> allStatistics;
        private readonly Dictionary<int, Routine> routines;
        private readonly List<Call> calls;
        private readonly Stack<Call> callStack;
        private TimeSpan runningTime;

        private readonly Dictionary<int, List<TimeSpan>> instructionTimings;
        private Stopwatch instructionTimer;

        private int routinesExecuted;
        private int instructionsExecuted;

        public ZMachineProfiler()
        {
            this.allStatistics = new List<RoutineCompilationStatistics>();
            this.routines = new Dictionary<int, Routine>();
            this.calls = new List<Call>();
            this.callStack = new Stack<Call>();

            this.instructionTimings = new Dictionary<int, List<TimeSpan>>();
            this.instructionTimer = new Stopwatch();
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
                routine = new Routine(this, address);
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
            instructionTimer.Restart();
        }

        void IZMachineProfiler.ExecutedInstruction(int address)
        {
            instructionTimer.Stop();

            List<TimeSpan> timingList;
            if (!instructionTimings.TryGetValue(address, out timingList))
            {
                timingList = new List<TimeSpan>();
                instructionTimings.Add(address, timingList);
            }

            timingList.Add(instructionTimer.Elapsed);

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

        public IEnumerable<Tuple<int, IEnumerable<TimeSpan>>> InstructionTimings
        {
            get
            {
                foreach (var timing in instructionTimings)
                {
                    var address = timing.Key;
                    var timings = (IEnumerable<TimeSpan>)timing.Value;
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
    }
}
