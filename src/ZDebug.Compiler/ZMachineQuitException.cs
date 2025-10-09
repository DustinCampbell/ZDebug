using System;

namespace ZDebug.Compiler;

public class ZMachineQuitException : Exception
{
    public ZMachineQuitException()
        : base()
    {
    }

    public ZMachineQuitException(string message)
        : base(message)
    {
    }

    public ZMachineQuitException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
