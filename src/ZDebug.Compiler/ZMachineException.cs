using System;

namespace ZDebug.Compiler
{
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
    }
}
