using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Core.Interpreter
{
    internal sealed class DefaultInterpreter : IInterpreter
    {
        public InterpreterTarget Target
        {
            get { return InterpreterTarget.IBMPC; }
        }

        public byte Version
        {
            get { return (byte)'A'; }
        }

        public byte StandardRevisionMajorVersion
        {
            get { return 1; }
        }

        public byte StandardRevisionMinorVersion
        {
            get { return 0; }
        }
    }
}
