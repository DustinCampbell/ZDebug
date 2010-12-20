using System;
using System.Runtime.Serialization;

namespace ZDebug.Core.Instructions
{
    [Serializable]
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

        protected OpcodeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
