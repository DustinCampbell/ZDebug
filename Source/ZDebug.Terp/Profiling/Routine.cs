namespace ZDebug.Terp.Profiling
{
    public sealed class Routine
    {
        private readonly int address;

        public Routine(int address)
        {
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
