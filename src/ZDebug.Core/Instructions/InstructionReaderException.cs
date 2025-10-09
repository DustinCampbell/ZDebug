using System;

namespace ZDebug.Core.Instructions;

public class InstructionReaderException : Exception
{
    public InstructionReaderException()
        : base()
    {
    }

    public InstructionReaderException(string message)
        : base(message)
    {
    }

    public InstructionReaderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
