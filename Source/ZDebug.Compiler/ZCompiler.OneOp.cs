using ZDebug.Compiler.Generate;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void op_dec()
        {
            using (var varIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<short>())
            {
                LoadByRefVariableOperand();
                varIndex.Store();

                LoadVariable(varIndex, indirect: true);
                il.Convert.ToInt16();
                il.Math.Subtract(1);
                value.Store();

                StoreVariable(varIndex, value, indirect: true);
            }
        }

        private void op_inc()
        {
            using (var varIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<short>())
            {
                LoadByRefVariableOperand();
                varIndex.Store();

                LoadVariable(varIndex, indirect: true);
                il.Convert.ToInt16();
                il.Math.Add(1);
                value.Store();

                StoreVariable(varIndex, value, indirect: true);
            }
        }

        private void op_get_child()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectChildFromOperand(0);
                result.Store();

                StoreVariable(currentInstruction.StoreVariable, result);

                result.Load();
                il.Load(0);
                il.Compare.GreaterThan();
                Branch();
            }
        }

        private void op_get_parent()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectParentFromOperand(0);
                result.Store();

                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_get_sibling()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectSiblingFromOperand(0);
                result.Store();

                StoreVariable(currentInstruction.StoreVariable, result);

                result.Load();
                il.Load(0);
                il.Compare.GreaterThan();
                Branch();
            }
        }

        private void op_remove_obj()
        {
            using (var objNum = il.NewLocal<ushort>())
            {
                LoadOperand(0);
                objNum.Store();

                RemoveObjectFromParent(objNum);
            }
        }

        private void op_get_prop_len()
        {
            using (var dataAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<byte>())
            {
                LoadOperand(0);
                dataAddress.Store();

                var done = il.NewLabel();
                var isNotZero = il.NewLabel();

                dataAddress.Load();
                isNotZero.BranchIf(Condition.True, @short: true);

                il.Load(0);
                value.Store();
                done.Branch();

                isNotZero.Mark();

                dataAddress.Load();
                il.Math.Subtract(1);
                il.Convert.ToUInt16();
                dataAddress.Store();

                LoadByte(dataAddress);
                value.Store();

                var checkForZero = il.NewLabel();

                if (machine.Version < 4)
                {
                    value.Load();
                    il.Math.Shr(5);
                    il.Math.Add(1);
                    il.Convert.ToUInt8();
                    value.Store();
                    checkForZero.Branch();
                }

                var secondBranch = il.NewLabel();

                value.Load();
                il.Math.And(0x80);
                secondBranch.BranchIf(Condition.True, @short: true);

                value.Load();
                il.Math.Shr(6);
                il.Math.Add(1);
                il.Convert.ToUInt8();
                value.Store();
                checkForZero.Branch();

                secondBranch.Mark();

                value.Load();
                il.Math.And(0x3f);
                il.Convert.ToUInt8();
                value.Store();

                checkForZero.Mark();

                value.Load();
                done.BranchIf(Condition.True, @short: true);

                il.Load(64);
                value.Store();

                done.Mark();

                StoreVariable(currentInstruction.StoreVariable, value);
            }
        }

        private void op_jump()
        {
            var address = currentInstruction.Address + currentInstruction.Length + (short)(currentInstruction.Operands[0].Value) - 2;
            var jump = addressToLabelMap[address];
            jump.Branch();
        }

        private void op_jz()
        {
            LoadOperand(0);
            il.Load(0);
            il.Compare.Equal();

            Branch();
        }

        private void op_load()
        {
            using (var varIndex = il.NewLocal<byte>())
            using (var result = il.NewLocal<ushort>())
            {
                LoadByRefVariableOperand();
                varIndex.Store();

                LoadVariable(varIndex, indirect: true);
                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_print_addr()
        {
            il.LoadArg(0);
            LoadOperand(0);

            var readZText = Reflection<ZMachine>.GetMethod("ReadZText", Types.One<int>(), @public: false);
            il.Call(readZText);
            PrintText();
        }

        private void op_print_paddr()
        {
            il.LoadArg(0);
            var op = GetOperand(0);
            LoadUnpackedStringAddress(op);

            var readZText = Reflection<ZMachine>.GetMethod("ReadZText", Types.One<int>(), @public: false);
            il.Call(readZText);
            PrintText();
        }

        private void op_print_obj()
        {
            il.LoadArg(0);
            ReadObjectShortNameFromOperand(0);

            var convertZText = Reflection<ZMachine>.GetMethod("ConvertZText", Types.One<ushort[]>(), @public: false);
            il.Call(convertZText);
            PrintText();
        }

        private void op_ret()
        {
            LoadOperand(0);
            Return();
        }
    }
}
