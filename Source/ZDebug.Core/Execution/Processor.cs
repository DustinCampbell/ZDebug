using System;
using System.Collections.Generic;
using ZDebug.Core.Basics;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Execution
{
    public sealed class Processor : IExecutionContext
    {
        private readonly Story story;
        private readonly IMemoryReader reader;
        private readonly InstructionReader instructions;
        private readonly Stack<StackFrame> callStack;

        private Instruction executingInstruction;

        internal Processor(Story story)
        {
            this.story = story;

            this.callStack = new Stack<StackFrame>();

            // create "call" to main routine
            var mainRoutineAddress = story.Memory.ReadMainRoutineAddress();
            this.reader = story.Memory.CreateReader(mainRoutineAddress);
            this.instructions = reader.AsInstructionReader(story.Version);

            var localCount = reader.NextByte();
            var locals = ArrayEx.Create(localCount, i => Value.Zero);

            callStack.Push(
                new StackFrame(
                    mainRoutineAddress,
                    arguments: ArrayEx.Empty<Value>(),
                    locals: locals,
                    returnAddress: null,
                    storeVariable: null));
        }

        private Value ReadVariable(Variable variable, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        return CurrentFrame.PeekValue();
                    }
                    else
                    {
                        return CurrentFrame.PopValue();
                    }

                case VariableKind.Local:
                    return CurrentFrame.Locals[variable.Index];

                case VariableKind.Global:
                    return story.GlobalVariablesTable[variable.Index];

                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteVariable(Variable variable, Value value, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    if (indirect)
                    {
                        CurrentFrame.PopValue();
                    }

                    CurrentFrame.PushValue(value);
                    break;

                case VariableKind.Local:
                    var oldValue = CurrentFrame.Locals[variable.Index];
                    CurrentFrame.SetLocal(variable.Index, value);
                    OnLocalVariableChanged(variable.Index, oldValue, value);
                    break;

                case VariableKind.Global:
                    story.GlobalVariablesTable[variable.Index] = value;
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        private Value GetOperandValue(Operand operand)
        {
            switch (operand.Kind)
            {
                case OperandKind.LargeConstant:
                    return operand.AsLargeConstant();
                case OperandKind.SmallConstant:
                    return operand.AsSmallConstant();
                case OperandKind.Variable:
                    return ReadVariable(operand.AsVariable());
                default:
                    throw new InvalidOperationException();
            }
        }

        private void WriteStoreVariable(Variable storeVariable, Value value)
        {
            if (storeVariable != null)
            {
                WriteVariable(storeVariable, value);
            }
        }

        private void Call(int address, Operand[] operands, Variable storeVariable)
        {
            if (address < 0)
            {
                throw new ArgumentOutOfRangeException("address");
            }

            // NOTE: argument values must be retrieved in case they manipulate the stack
            var argValues = operands.Select(GetOperandValue);

            if (address == 0)
            {
                // SPECIAL CASE: A routine call to packed address 0 is legal: it does nothing and returns false (0). Otherwise it is
                // illegal to call a packed address where no routine is present.

                // If there is a store variable, write 0 to it.
                WriteStoreVariable(storeVariable, Value.Zero);
            }
            else
            {
                story.RoutineTable.Add(address);

                var returnAddress = reader.Address;
                reader.Address = address;

                // read locals
                var localCount = reader.NextByte();
                var locals = story.Version <= 4
                    ? ArrayEx.Create(localCount, _ => Value.Number(reader.NextWord()))
                    : ArrayEx.Create(localCount, _ => Value.Zero);

                var numberToCopy = Math.Min(argValues.Length, locals.Length);
                Array.Copy(argValues, 0, locals, 0, numberToCopy);

                var oldFrame = CurrentFrame;
                var newFrame = new StackFrame(address, argValues, locals, returnAddress, storeVariable);

                callStack.Push(newFrame);

                OnEnterFrame(oldFrame, newFrame);
            }
        }

        private void Jump(short offset)
        {
            reader.Address += offset - 2;
        }

        private void Jump(Branch branch)
        {
            if (branch.Kind == BranchKind.Address)
            {
                reader.Address += branch.Offset - 2;
            }
            else if (branch.Kind == BranchKind.RFalse)
            {
                Return(Value.Zero);
            }
            else if (branch.Kind == BranchKind.RTrue)
            {
                Return(Value.One);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private void Return(Value value)
        {
            var oldFrame = callStack.Pop();

            OnExitFrame(oldFrame, CurrentFrame);

            reader.Address = oldFrame.ReturnAddress;

            WriteStoreVariable(oldFrame.StoreVariable, value);
        }

        private void WriteProperty(int objNum, int propNum, ushort value)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);
            var prop = obj.PropertyTable.GetByNumber(propNum);

            if (prop.DataLength == 2)
            {
                story.Memory.WriteWord(prop.DataAddress, value);
            }
            else if (prop.DataLength == 1)
            {
                story.Memory.WriteByte(prop.DataAddress, (byte)(value & 0x00ff));
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private bool HasAttribute(int objNum, int attrNum)
        {
            var obj = story.ObjectTable.GetByNumber(objNum);

            return obj.HasAttribute(attrNum);
        }

        public void Step()
        {
            var oldPC = reader.Address;
            executingInstruction = instructions.NextInstruction();
            OnStepping(oldPC);

            executingInstruction.Opcode.Execute(executingInstruction, this);

            var newPC = reader.Address;
            OnStepped(oldPC, newPC);
            executingInstruction = null;
        }

        public StackFrame CurrentFrame
        {
            get { return callStack.Peek(); }
        }

        public int PC
        {
            get { return reader.Address; }
        }

        /// <summary>
        /// The Instruction that is being executed (only valid during a step).
        /// </summary>
        public Instruction ExecutingInstruction
        {
            get { return executingInstruction; }
        }

        private void OnStepping(int oldPC)
        {
            var handler = Stepping;
            if (handler != null)
            {
                handler(this, new ProcessorSteppingEventArgs(oldPC));
            }
        }

        private void OnStepped(int oldPC, int newPC)
        {
            var handler = Stepped;
            if (handler != null)
            {
                handler(this, new ProcessorSteppedEventArgs(oldPC, newPC));
            }
        }

        private void OnEnterFrame(StackFrame oldFrame, StackFrame newFrame)
        {
            var handler = EnterFrame;
            if (handler != null)
            {
                handler(this, new StackFrameEventArgs(oldFrame, newFrame));
            }
        }

        private void OnExitFrame(StackFrame oldFrame, StackFrame newFrame)
        {
            var handler = ExitFrame;
            if (handler != null)
            {
                handler(this, new StackFrameEventArgs(oldFrame, newFrame));
            }
        }

        private void OnLocalVariableChanged(int index, Value oldValue, Value newValue)
        {
            var handler = LocalVariableChanged;
            if (handler != null)
            {
                handler(this, new LocalVariableChangedEventArgs(index, oldValue, newValue));
            }
        }

        public event EventHandler<ProcessorSteppingEventArgs> Stepping;
        public event EventHandler<ProcessorSteppedEventArgs> Stepped;

        public event EventHandler<StackFrameEventArgs> EnterFrame;
        public event EventHandler<StackFrameEventArgs> ExitFrame;

        public event EventHandler<LocalVariableChangedEventArgs> LocalVariableChanged;

        Value IExecutionContext.GetOperandValue(Operand operand)
        {
            return GetOperandValue(operand);
        }

        Value IExecutionContext.ReadByte(int address)
        {
            return Value.Number(story.Memory.ReadByte(address));
        }

        Value IExecutionContext.ReadVariable(Variable variable)
        {
            return ReadVariable(variable);
        }

        Value IExecutionContext.ReadVariableIndirectly(Variable variable)
        {
            return ReadVariable(variable, indirect: true);
        }

        Value IExecutionContext.ReadWord(int address)
        {
            return Value.Number(story.Memory.ReadWord(address));
        }

        void IExecutionContext.WriteByte(int address, byte value)
        {
            story.Memory.WriteByte(address, value);
        }

        void IExecutionContext.WriteProperty(int objNum, int propNum, ushort value)
        {
            WriteProperty(objNum, propNum, value);
        }

        void IExecutionContext.WriteVariable(Variable variable, Value value)
        {
            WriteVariable(variable, value);
        }

        void IExecutionContext.WriteVariableIndirectly(Variable variable, Value value)
        {
            WriteVariable(variable, value, indirect: true);
        }

        void IExecutionContext.WriteWord(int address, ushort value)
        {
            story.Memory.WriteWord(address, value);
        }

        void IExecutionContext.Call(int address, Operand[] operands, Variable storeVariable)
        {
            Call(address, operands, storeVariable);
        }

        void IExecutionContext.Jump(short offset)
        {
            Jump(offset);
        }

        void IExecutionContext.Jump(Branch branch)
        {
            Jump(branch);
        }

        void IExecutionContext.Return(Value value)
        {
            Return(value);
        }

        int IExecutionContext.UnpackRoutineAddress(ushort byteAddress)
        {
            return story.UnpackRoutineAddress(byteAddress);
        }

        bool IExecutionContext.HasAttribute(int objNum, int attrNum)
        {
            return HasAttribute(objNum, attrNum);
        }
    }
}
