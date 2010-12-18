using System;

namespace ZDebug.Core.Execution
{
    public interface IInputStream
    {
        void ReadChar(Action<char> callback);
        void ReadCommand(int maxChars, Action<string> callback);
    }
}
