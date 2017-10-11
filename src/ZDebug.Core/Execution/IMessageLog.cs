namespace ZDebug.Core.Execution
{
    public interface IMessageLog
    {
        void SendWarning(string message);
        void SendError(string message);
    }
}
