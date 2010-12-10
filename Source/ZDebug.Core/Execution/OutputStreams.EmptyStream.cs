namespace ZDebug.Core.Execution
{
    public sealed partial class OutputStreams
    {
        private class EmptyStream : IOutputStream
        {
            private EmptyStream()
            {
            }

            public void Print(string text)
            {
            }

            public void Print(char ch)
            {
            }

            public static readonly IOutputStream Instance = new EmptyStream();
        }
    }
}
