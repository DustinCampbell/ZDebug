using System;
using System.Runtime.Serialization;

namespace ZDebug.Compiler
{
    [Serializable]
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

        protected ZMachineQuitException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
