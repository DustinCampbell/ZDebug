namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private class NullStream : IOutputStream
        {
            private NullStream()
            {
            }

            public void Print(string text)
            {
            }

            public void Print(char ch)
            {
            }

            public static readonly IOutputStream Instance = new NullStream();
        }
    }
}
