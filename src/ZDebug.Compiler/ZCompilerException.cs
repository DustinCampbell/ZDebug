using System;
using System.Runtime.Serialization;

namespace ZDebug.Compiler
{
    [Serializable]
    public class ZCompilerException : Exception
    {
        public ZCompilerException()
            : base()
        {
        }

        public ZCompilerException(string message)
            : base(message)
        {
        }

        public ZCompilerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ZCompilerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}