namespace ZDebug.Core.Execution
{
    internal sealed partial class OutputStreams
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
