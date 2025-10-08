using System;

namespace ZDebug.Core.Blorb;

public class BlorbFileException : Exception
{
    public BlorbFileException()
        : base()
    {
    }

    public BlorbFileException(string message)
        : base(message)
    {
    }

    public BlorbFileException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
