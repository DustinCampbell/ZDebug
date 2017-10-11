using System;
using System.Runtime.Serialization;

namespace ZDebug.Core.Blorb
{
    [Serializable]
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

        protected BlorbFileException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
