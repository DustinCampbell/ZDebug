namespace ZDebug.Core.Execution
{
    public sealed partial class Processor
    {
        private sealed class NullScreen : IScreen
        {
            public void Clear(int window)
            {
            }

            public void ClearAll(bool unsplit = false)
            {
            }

            public void Print(string text)
            {
            }

            public void Print(char ch)
            {
            }
        }
    }
}
