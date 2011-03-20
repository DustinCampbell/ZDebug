using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;
using System.Collections.ObjectModel;
using ZDebug.Core.Extensions;

namespace ZDebug.Core.Execution
{
    public sealed class StackFrame
    {
        private readonly uint callAddress;
        private readonly ReadOnlyCollection<ushort> arguments;
        private readonly ReadOnlyCollection<ushort> locals;
        private readonly uint returnAddress;
        private readonly Variable storeVariable;

        internal StackFrame(uint callAddress, ushort[] arguments, ushort[] locals, uint returnAddress, Variable storeVariable)
        {
            this.callAddress = callAddress;
            this.arguments = arguments.AsReadOnly();
            this.locals = locals.AsReadOnly();
            this.returnAddress = returnAddress;
            this.storeVariable = storeVariable;
        }

        public uint CallAddress
        {
            get { return callAddress; }
        }

        public ReadOnlyCollection<ushort> Arguments
        {
            get { return arguments; }
        }

        public ReadOnlyCollection<ushort> Locals
        {
            get { return locals; }
        }

        public uint ReturnAddress
        {
            get { return returnAddress; }
        }

        public Variable StoreVariable
        {
            get { return storeVariable; }
        }
    }
}
