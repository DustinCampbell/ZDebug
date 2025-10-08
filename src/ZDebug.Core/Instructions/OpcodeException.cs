using System;

namespace ZDebug.Core.Instructions;

public class OpcodeException : Exception
{
    public OpcodeException()
        : base()
    {
    }

    public OpcodeException(string message)
        : base(message)
    {
    }

    public OpcodeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
