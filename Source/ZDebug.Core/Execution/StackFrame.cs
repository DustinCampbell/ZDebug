using System;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class StackFrame
    {
        private const int stackSize = 1024;

        private int address;
        private Value[] arguments;
        private Value[] initialLocalValues;
        private Value[] locals;
        private int? returnAddress;
        private Variable storeVariable;

        private Value[] stack;
        private int sp;

        internal StackFrame(
            int address,
            Value[] arguments,
            Value[] locals,
            int? returnAddress,
            Variable storeVariable)
        {
            Initialize(address, arguments, locals, returnAddress, storeVariable);
        }

        internal void Initialize(
            int address,
            Value[] arguments,
            Value[] locals,
            int? returnAddress,
            Variable storeVariable)
        {
            this.address = address;
            this.arguments = arguments;
            this.initialLocalValues = locals.ShallowCopy();
            this.locals = locals;
            this.returnAddress = returnAddress;
            this.storeVariable = storeVariable;

            this.stack = new Value[stackSize];
            sp = stackSize;
        }

        public Value PopValue()
        {
            if (sp == stackSize)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            return stack[sp++];
        }

        public Value PeekValue()
        {
            if (sp == stackSize)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            return stack[sp];
        }

        public void PushValue(Value value)
        {
            if (sp == 0)
            {
                throw new InvalidOperationException("Stack underflow");
            }

            stack[--sp] = value;
        }

        public void SetLocal(int index, Value value)
        {
            locals[index] = value;
        }

        public int Address
        {
            get { return address; }
        }
        public Value[] Arguments
        {
            get { return arguments; }
        }
        public Value[] InitialLocalValues
        {
            get { return initialLocalValues; }
        }
        public Value[] Locals
        {
            get { return locals; }
        }

        public bool HasReturnAddress
        {
            get { return returnAddress.HasValue; }
        }

        public int ReturnAddress
        {
            get { return returnAddress.Value; }
        }

        public bool HasStoreVariable
        {
            get { return storeVariable != null; }
        }

        public Variable StoreVariable
        {
            get { return storeVariable; }
        }
    }
}
