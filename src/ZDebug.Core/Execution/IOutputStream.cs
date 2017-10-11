namespace ZDebug.Core.Execution
{
    public interface IOutputStream
    {
        void Print(string text);
        void Print(char ch);
    }
}
