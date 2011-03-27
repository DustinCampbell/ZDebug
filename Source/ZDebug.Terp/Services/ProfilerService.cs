using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using ZDebug.Terp.Profiling;
using ZDebug.UI.Services;

namespace ZDebug.Terp.Services
{
    [Export]
    internal sealed class ProfilerService : IService
    {
        private ZMachineProfiler profiler;
        private Stopwatch watch;

        private TimeSpan compileTime;
        private int routinesCompiled;
        private double zcodeToILRatio;
        private int routinesExecuted;
        private int instructionsExecuted;
        private int calculatedVariableLoads;
        private int calculatedVariableStores;
        private int directCalls;
        private int calculatedCalls;

        [ImportingConstructor]
        private ProfilerService()
        {
        }

        private void OnStarting()
        {
            var handler = Starting;
            if (handler != null)
            {
                handler(this, new ProfilerStartingEventArgs());
            }
        }

        private void OnStopped()
        {
            var handler = Stopped;
            if (handler != null)
            {
                handler(this, new ProfilerStoppedEventArgs());
            }
        }

        public void Create()
        {
            this.profiler = new ZMachineProfiler();
        }

        public void Destroy()
        {
            this.profiler = null;
        }

        public void Start()
        {
            OnStarting();
            watch = Stopwatch.StartNew();
        }

        public void Stop()
        {
            watch.Stop();
            profiler.Stop(watch.Elapsed);
            OnStopped();
        }

        public void UpdateProfilerStatistics()
        {
            if (profiler == null)
            {
                return;
            }

            var compilationStatistics = profiler.CompilationStatistics.ToList();
            if (compilationStatistics.Count > 0)
            {
                this.compileTime = new TimeSpan(compilationStatistics.Sum(s => s.CompileTime.Ticks));
                this.routinesCompiled = profiler.RoutinesCompiled;
                this.zcodeToILRatio = compilationStatistics.Select(s => (double)s.Size / (double)s.Routine.Length).Average();
                this.routinesExecuted = profiler.RoutinesExecuted;
                this.instructionsExecuted = profiler.InstructionsExecuted;
                this.calculatedVariableLoads = compilationStatistics.Sum(s => s.CalculatedLoadVariableCount);
                this.calculatedVariableStores = compilationStatistics.Sum(s => s.CalculatedStoreVariableCount);
                this.directCalls = profiler.DirectCallCount;
                this.calculatedCalls = profiler.CalculatedCallCount;
            }
            else
            {
                this.compileTime = TimeSpan.Zero;
                this.routinesCompiled = 0;
                this.zcodeToILRatio = 0;
                this.routinesCompiled = 0;
                this.instructionsExecuted = 0;
                this.calculatedVariableLoads = 0;
                this.calculatedVariableStores = 0;
                this.directCalls = 0;
                this.calculatedCalls = 0;
            }
        }

        public bool Profiling
        {
            get
            {
                return profiler != null;
            }
        }

        public TimeSpan Elapsed
        {
            get
            {
                return watch.Elapsed;
            }
        }

        public ZMachineProfiler Profiler
        {
            get
            {
                return profiler;
            }
        }

        public TimeSpan CompileTime
        {
            get
            {
                return compileTime;
            }
        }

        public int RoutinesCompiled
        {
            get
            {
                return routinesCompiled;
            }
        }

        public double ZCodeToILRatio
        {
            get
            {
                return zcodeToILRatio;
            }
        }

        public double ZCodeToILRatioPercent
        {
            get
            {
                return zcodeToILRatio * 100;
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

        public int CalculatedVariableLoads
        {
            get
            {
                return calculatedVariableLoads;
            }
        }

        public int CalculatedVariableStores
        {
            get
            {
                return calculatedVariableStores;
            }
        }

        public int DirectCalls
        {
            get
            {
                return directCalls;
            }
        }

        public int CalculatedCalls
        {
            get
            {
                return calculatedCalls;
            }
        }

        public event EventHandler<ProfilerStartingEventArgs> Starting;
        public event EventHandler<ProfilerStoppedEventArgs> Stopped;
    }
}
