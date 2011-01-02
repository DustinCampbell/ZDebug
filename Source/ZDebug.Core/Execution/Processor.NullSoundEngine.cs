namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private class NullSoundEngine : ISoundEngine
        {
            private NullSoundEngine()
            {
            }

            public void Beep()
            {
            }

            public static readonly ISoundEngine Instance = new NullSoundEngine();
        }
    }
}
