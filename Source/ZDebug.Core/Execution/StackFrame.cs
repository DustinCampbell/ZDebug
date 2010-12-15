using System;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class StackFrame
    {
        private const int stackSize = 1024;

        public readonly int Address;
        public readonly Value[] Arguments;
        public readonly Value[] InitialLocalValues;
        public readonly Value[] Locals;
        private readonly int? returnAddress;
        private readonly Variable storeVariable;

        private readonly Value[] stack;
        private int sp;

        internal StackFrame(
            int address,
            Value[] arguments,
            Value[] locals,
            int? returnAddress,
            Variable storeVariable)
        {
            this.Address = address;
            this.Arguments = arguments;
            this.InitialLocalValues = locals.ShallowCopy();
            this.Locals = locals;
            this.stack = new Value[stackSize];
            sp = stackSize;
            this.returnAddress = returnAddress;
            this.storeVariable = storeVariable;
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
            Locals[index] = value;
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
