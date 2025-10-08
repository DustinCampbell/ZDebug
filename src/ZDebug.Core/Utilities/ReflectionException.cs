using System;

namespace ZDebug.Core.Utilities
{
    public class ReflectionException : Exception
    {
        public ReflectionException()
            : base()
        {
        }

        public ReflectionException(string message)
            : base(message)
        {
        }

        public ReflectionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
