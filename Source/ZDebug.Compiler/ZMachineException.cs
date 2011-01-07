using System;
using System.Runtime.Serialization;

namespace ZDebug.Compiler
{
    [Serializable]
    public class ZMachineException : Exception
    {
        public ZMachineException()
            : base()
        {
        }

        public ZMachineException(string message)
            : base(message)
        {
        }

        public ZMachineException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ZMachineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
