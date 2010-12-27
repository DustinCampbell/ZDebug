using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;
using System.Collections.ObjectModel;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class StackFrame
    {
        private readonly uint callAddress;
        private readonly int argumentCount;
        private readonly ReadOnlyCollection<ushort> locals;
        private readonly int returnAddress;
        private readonly Variable storeVariable;

        internal StackFrame(uint callAddress, int argumentCount, ushort[] locals, int returnAddress, Variable storeVariable)
        {
            this.callAddress = callAddress;
            this.argumentCount = argumentCount;
            this.locals = locals.AsReadOnly();
            this.returnAddress = returnAddress;
            this.storeVariable = storeVariable;
        }

        public uint CallAddress
        {
            get { return callAddress; }
        }

        public int ArgumentCount
        {
            get { return argumentCount; }
        }

        public ReadOnlyCollection<ushort> Locals
        {
            get { return locals; }
        }

        public int ReturnAddress
        {
            get { return returnAddress; }
        }

        public Variable StoreVariable
        {
            get { return storeVariable; }
        }
    }
}
