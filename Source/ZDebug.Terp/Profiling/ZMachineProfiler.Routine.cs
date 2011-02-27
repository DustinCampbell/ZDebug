namespace ZDebug.Terp.Profiling
{
    public partial class ZMachineProfiler
    {
        private sealed class Routine : IRoutine
        {
            private readonly ZMachineProfiler profiler;
            private readonly int address;

            public Routine(ZMachineProfiler profiler, int address)
            {
                this.profiler = profiler;
                this.address = address;
            }

            public int Address
            {
                get
                {
                    return address;
                }
            }
        }
    }
}
