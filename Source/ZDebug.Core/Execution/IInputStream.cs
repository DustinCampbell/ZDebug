using System;

namespace ZDebug.Core.Execution
{
    public interface IInputStream
    {
        void ReadChar(Action<char> callback);
    }
}
