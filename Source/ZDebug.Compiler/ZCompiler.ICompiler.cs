using ZDebug.Compiler.CodeGeneration;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    public partial class ZCompiler : ICompiler
    {
        public ILabel GetLabel(int address)
        {
            ILabel result;
            if (addressToLabelMap.TryGetValue(address, out result))
            {
                return result;
            }

            throw new ZCompilerException(string.Format("Could not find label for address, {0:x4}", address));
        }

        public void EmitLoadOperand(Operand operand)
        {
            switch (operand.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    il.Load(operand.Value);
                    break;

                default: // OperandKind.Variable
                    LoadVariable((byte)operand.Value);
                    break;
            }
        }

        public void EmitBranch(Branch branch)
        {
            // It is expected that the value on the top of the evaluation stack
            // is the boolean value to compare branch.Condition with.

            var noJump = il.NewLabel();

            il.Load(branch.Condition);
            noJump.BranchIf(Condition.NotEqual, @short: true);

            switch (branch.Kind)
            {
                case BranchKind.RFalse:
                    il.Load(0);
                    Return();
                    break;

                case BranchKind.RTrue:
                    il.Load(1);
                    Return();
                    break;

                default: // BranchKind.Address
                    var jump = GetLabel(branch.TargetAddress);
                    jump.Branch();
                    break;
            }

            noJump.Mark();
        }

        public void EmitReturn()
        {
            var popFrame = Reflection<CompiledZMachine>.GetMethod("PopFrame", @public: false);

            il.LoadThis();
            il.Call(popFrame);

            il.Return();
        }

        public void EmitCall()
        {
            var addressOp = GetOperand(0);
            if (addressOp.Kind != OperandKind.Variable)
            {
                DirectCall(addressOp);
            }
            else
            {
                CalculatedCall(addressOp);
            }
        }

        private void EmitLoadMemoryByte(CodeBuilder loadAddress)
        {
            memory.LoadElement(loadAddress);
        }

        public void EmitLoadMemoryByte(int address)
        {
            EmitLoadMemoryByte(
                loadAddress: () => il.Load(address));
        }

        public void EmitLoadMemoryByte(ILocal address)
        {
            EmitLoadMemoryByte(
                loadAddress: () => address.Load());
        }

        private void EmitLoadMemoryWord(CodeBuilder loadAddress)
        {
            // shift memory[address] left 8 bits
            memory.LoadElement(loadAddress);
            il.Math.Shl(8);

            // read memory[address + 1]
            memory.LoadElement(() =>
            {
                loadAddress();
                il.Math.Add(1);
            });

            // or bytes together
            il.Math.Or();
            il.Convert.ToUInt16();
        }

        public void EmitLoadMemoryWord(int address)
        {
            EmitLoadMemoryWord(
                loadAddress: () => il.Load(address));
        }

        public void EmitLoadMemoryWord(ILocal address)
        {
            EmitLoadMemoryWord(
                loadAddress: () => address.Load());
        }

        private void EmitStoreMemoryByte(CodeBuilder loadAddress, CodeBuilder loadValue)
        {
            memory.StoreElement(loadAddress, loadValue);
        }

        public void EmitStoreMemoryByte(int address, ILocal value)
        {
            EmitStoreMemoryByte(
                loadAddress: () => il.Load(address),
                loadValue: () => value.Load());
        }

        public void EmitStoreMemoryByte(ILocal address, ILocal value)
        {
            EmitStoreMemoryByte(
                loadAddress: () => address.Load(),
                loadValue: () => value.Load());
        }

        private void EmitStoreMemoryWord(CodeBuilder loadAddress, CodeBuilder loadValue)
        {
            // memory[address] = (byte)(value >> 8);
            memory.StoreElement(
                loadAddress,
                () =>
                {
                    loadValue();
                    il.Math.Shr(8);
                    il.Convert.ToUInt8();
                });

            // memory[address + 1] = (byte)(value & 0xff);
            memory.StoreElement(
                () =>
                {
                    loadAddress();
                    il.Math.Add(1);
                },
                () =>
                {
                    loadValue();
                    il.Math.And(0xff);
                    il.Convert.ToUInt8();
                });
        }

        public void EmitStoreMemoryWord(int address, ILocal value)
        {
            EmitStoreMemoryWord(
                loadAddress: () => il.Load(address),
                loadValue: () => value.Load());
        }

        public void EmitStoreMemoryWord(ILocal address, ILocal value)
        {
            EmitStoreMemoryWord(
                loadAddress: () => address.Load(),
                loadValue: () => value.Load());
        }

        public void EmitPopStack(bool indirect = false)
        {
            stack.LoadElement(
                indexLoader: () =>
                {
                    spRef.Load();
                    spRef.LoadIndirectValue();
                });

            if (!indirect)
            {
                // decrement sp
                spRef.Load();
                spRef.Load();
                spRef.LoadIndirectValue();
                il.Math.Subtract(1);
                spRef.StoreIndirectValue();
            }
        }

        public void EmitPushStack(ILocal value, bool indirect = false)
        {
            if (!indirect)
            {
                // increment sp
                spRef.Load();
                spRef.Load();
                spRef.LoadIndirectValue();
                il.Math.Add(1);
                spRef.StoreIndirectValue();
            }

            stack.StoreElement(
                indexLoader: () =>
                {
                    spRef.Load();
                    spRef.LoadIndirectValue();
                },
                valueLoader: () =>
                {
                    value.Load();
                });
        }

        private void EmitLoadLocalVariable(byte variableIndex)
        {
            localRefs[variableIndex].Load();
            localRefs[variableIndex].LoadIndirectValue();
        }

        private void EmitLoadLocalVariable(ILocal variableIndex)
        {
            locals.LoadElement(
                indexLoader: () => variableIndex.Load());
        }

        private void EmitStoreLocalVariable(byte variableIndex, ILocal value)
        {
            localRefs[variableIndex].Load();
            value.Load();
            localRefs[variableIndex].StoreIndirectValue();
        }

        private void EmitStoreLocalVariable(ILocal variableIndex, ILocal value)
        {
            locals.StoreElement(
                indexLoader: () => variableIndex.Load(),
                valueLoader: () => value.Load());
        }

        private void EmitLoadGlobalVariable(byte variableIndex)
        {
            var address = (variableIndex * 2) + machine.GlobalVariableTableAddress;
            EmitLoadMemoryWord(address);
        }

        private void EmitLoadGlobalVariable(ILocal variableIndex)
        {
            using (var address = il.NewLocal<int>())
            {
                variableIndex.Load();
                il.Math.Multiply(2);
                il.Math.Add(machine.GlobalVariableTableAddress);
                address.Store();

                EmitLoadMemoryWord(address);
            }
        }

        private void EmitStoreGlobalVariable(byte variableIndex, ILocal value)
        {
            var address = (variableIndex * 2) + machine.GlobalVariableTableAddress;
            EmitStoreMemoryWord(address, value);
        }

        private void EmitStoreGlobalVariable(ILocal variableIndex, ILocal value)
        {
            using (var address = il.NewLocal<int>())
            {
                variableIndex.Load();
                il.Math.Multiply(2);
                il.Math.Add(machine.GlobalVariableTableAddress);
                address.Store();

                EmitStoreMemoryWord(address, value);
            }
        }

        public void EmitLoadVariable(byte variableIndex, bool indirect = false)
        {
            if (variableIndex == 0)
            {
                EmitPopStack(indirect);
            }
            else if (variableIndex < 16)
            {
                EmitLoadLocalVariable((byte)(variableIndex - 1));
            }
            else
            {
                EmitLoadGlobalVariable((byte)(variableIndex - 16));
            }
        }

        public void EmitLoadVariable(ILocal variableIndex, bool indirect = false)
        {
            var tryLocal = il.NewLabel();
            var tryGlobal = il.NewLabel();
            var done = il.NewLabel();

            // branch if this is not the stack (variableIndex > 0)
            variableIndex.Load();
            tryLocal.BranchIf(Condition.True, @short: true);

            // stack
            if (usesStack)
            {
                EmitPopStack(indirect);
                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected stack access.");
            }

            // local
            tryLocal.Mark();

            // branch if this is not a local (variableIndex >= 16)
            variableIndex.Load();
            il.Load(16);
            tryGlobal.BranchIf(Condition.AtLeast, @short: true);

            if (localRefs != null)
            {
                using (var adjustedVariableIndex = il.NewLocal<byte>())
                {
                    variableIndex.Load();
                    il.Math.Subtract(1);
                    adjustedVariableIndex.Store();

                    EmitLoadLocalVariable(adjustedVariableIndex);
                }

                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected read from local variable {0}.", variableIndex);
            }

            // global
            tryGlobal.Mark();

            if (memory != null)
            {
                using (var adjustedVariableIndex = il.NewLocal<byte>())
                {
                    variableIndex.Load();
                    il.Math.Subtract(16);
                    adjustedVariableIndex.Store();

                    EmitLoadGlobalVariable(adjustedVariableIndex);
                }
            }
            else
            {
                il.RuntimeError("Unexpected global variable access.");
            }

            done.Mark();
        }

        public void EmitStoreVariable(byte variableIndex, ILocal value, bool indirect = false)
        {
            if (variableIndex == 0)
            {
                EmitPushStack(value, indirect);
            }
            else if (variableIndex < 16)
            {
                EmitStoreLocalVariable((byte)(variableIndex - 1), value);
            }
            else
            {
                EmitStoreGlobalVariable((byte)(variableIndex - 16), value);
            }
        }

        public void EmitStoreVariable(Variable variable, ILocal value, bool indirect = false)
        {
            switch (variable.Kind)
            {
                case VariableKind.Stack:
                    EmitPushStack(value, indirect);
                    break;

                case VariableKind.Local:
                    EmitStoreLocalVariable(variable.Index, value);
                    break;

                case VariableKind.Global:
                    EmitStoreGlobalVariable(variable.Index, value);
                    break;
            }
        }

        public void EmitStoreVariable(ILocal variableIndex, ILocal value, bool indirect = false)
        {
            var tryLocal = il.NewLabel();
            var tryGlobal = il.NewLabel();
            var done = il.NewLabel();

            // branch if this is not the stack (variableIndex > 0)
            variableIndex.Load();
            tryLocal.BranchIf(Condition.True, @short: true);

            // stack
            if (usesStack)
            {
                EmitPushStack(value, indirect);
                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected stack access.");
            }

            // local
            tryLocal.Mark();

            // branch if this is not a local (variableIndex >= 16)
            variableIndex.Load();
            il.Load(16);
            tryGlobal.BranchIf(Condition.AtLeast, @short: true);

            if (localRefs != null)
            {
                using (var adjustedVariableIndex = il.NewLocal<byte>())
                {
                    variableIndex.Load();
                    il.Math.Subtract(1);
                    adjustedVariableIndex.Store();

                    EmitStoreLocalVariable(adjustedVariableIndex, value);
                }

                done.Branch(@short: true);
            }
            else
            {
                il.RuntimeError("Unexpected write to local variable {0}.", variableIndex);
            }

            // global
            tryGlobal.Mark();

            if (memory != null)
            {
                using (var adjustedVariableIndex = il.NewLocal<byte>())
                {
                    variableIndex.Load();
                    il.Math.Subtract(16);
                    adjustedVariableIndex.Store();

                    EmitStoreGlobalVariable(adjustedVariableIndex, value);
                }
            }
            else
            {
                il.RuntimeError("Unexpected global variable access.");
            }

            done.Mark();
        }

        private void EmitLoadObjectParent(int objNum)
        {
            var address = (ushort)(CalculateObjectAddress(objNum) + machine.ObjectParentOffset);

            ReadObjectNumber(address);
        }

        public void EmitLoadObjectParent(Operand operand)
        {
            switch (operand.Kind)
            {
                case OperandKind.LargeConstant:
                case OperandKind.SmallConstant:
                    ReadObjectParent(operand.Value);
                    break;

                case OperandKind.Variable:
                    EmitLoadOperand(operand);
                    ReadObjectParent();
                    break;
            }
        }
    }
}
