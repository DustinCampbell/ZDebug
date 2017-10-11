using System;
using System.Runtime.Serialization;

namespace ZDebug.Core.Utilities
{
    [Serializable]
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

        protected ReflectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
