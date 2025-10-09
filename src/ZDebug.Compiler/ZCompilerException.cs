using System;

namespace ZDebug.Compiler;

public class ZCompilerException : Exception
{
    public ZCompilerException()
        : base()
    {
    }

    public ZCompilerException(string message)
        : base(message)
    {
    }

    public ZCompilerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}