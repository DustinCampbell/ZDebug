using System;
using System.Runtime.Serialization;

namespace ZDebug.Core.Instructions
{
    [Serializable]
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

        protected InstructionReaderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
