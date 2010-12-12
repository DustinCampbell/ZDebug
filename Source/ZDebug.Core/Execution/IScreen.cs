namespace ZDebug.Core.Execution
{
    public interface IScreen : IOutputStream
    {
        void Clear(int window);
        void ClearAll(bool unsplit = false);

        void SetTextStyle(ZTextStyle style);
    }
}
