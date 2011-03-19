namespace ZDebug.Core.Execution
{
    public abstract partial class ZMachine
    {
        private class NullSoundEngine : ISoundEngine
        {
            private NullSoundEngine()
            {
            }

            public void HighBeep()
            {
            }

            public void LowBeep()
            {
            }

            public static readonly ISoundEngine Instance = new NullSoundEngine();
        }
    }
}
