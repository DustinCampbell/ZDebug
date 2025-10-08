using System;

namespace ZDebug.Compiler
{
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
    }
}
