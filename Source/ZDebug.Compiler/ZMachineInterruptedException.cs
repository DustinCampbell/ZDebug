using System;
using System.Runtime.Serialization;

namespace ZDebug.Compiler
{
    [Serializable]
    public class ZMachineInterruptedException : Exception
    {
        public ZMachineInterruptedException()
            : base()
        {
        }

        public ZMachineInterruptedException(string message)
            : base(message)
        {
        }

        public ZMachineInterruptedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ZMachineInterruptedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
