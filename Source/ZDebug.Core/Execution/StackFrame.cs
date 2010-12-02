using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class StackFrame
    {
        private readonly int address;
        private readonly ReadOnlyCollection<Value> arguments;
        private readonly ReadOnlyCollection<Value> initialLocalValues;
        private readonly Value[] locals;
        private readonly ReadOnlyCollection<Value> readOnlyLocals;
        private readonly Stack<Value> stack;
        private readonly int? returnAddress;
        private readonly Variable storeVariable;

        internal StackFrame(
            int address,
            Value[] arguments,
            Value[] locals,
            int? returnAddress,
            Variable storeVariable)
        {
            this.address = address;
            this.arguments = arguments.AsReadOnly();
            this.initialLocalValues = locals.ShallowCopy().AsReadOnly();
            this.locals = locals;
            this.readOnlyLocals = this.locals.AsReadOnly();
            this.stack = new Stack<Value>();
            this.returnAddress = returnAddress;
            this.storeVariable = storeVariable;
        }

        public Value PopValue()
        {
            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            return stack.Pop();
        }

        public Value PeekValue()
        {
            if (stack.Count == 0)
            {
                throw new InvalidOperationException("Stack is empty.");
            }

            return stack.Peek();
        }

        public void PushValue(Value value)
        {
            stack.Push(value);
        }

        public int Address
        {
            get { return this.address; }
        }

        public ReadOnlyCollection<Value> Arguments
        {
            get { return this.arguments; }
        }

        public ReadOnlyCollection<Value> InitialLocalValues
        {
            get { return this.initialLocalValues; }
        }

        public ReadOnlyCollection<Value> Locals
        {
            get { return this.readOnlyLocals; }
        }

        public void SetLocal(int index, Value value)
        {
            locals[index] = value;
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
