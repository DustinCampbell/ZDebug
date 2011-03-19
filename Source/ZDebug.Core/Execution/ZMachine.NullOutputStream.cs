namespace ZDebug.Core.Execution
{
    public abstract partial class ZMachine
    {
        private class NullOutputStream : IOutputStream
        {
            private NullOutputStream()
            {
            }

            public void Print(string text)
            {
            }

            public void Print(char ch)
            {
            }

            public static readonly IOutputStream Instance = new NullOutputStream();
        }
    }
}
