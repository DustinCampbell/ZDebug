using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;
using System.Reflection.Emit;
using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void op_dec()
        {
            using (var value = il.NewLocal<short>())
            {
                var variable = ReadByRefVariableOperand();

                ReadVariable(variable, indirect: true);
                il.ConvertToInt16();
                il.Subtract(1);
                value.Store();

                WriteVariable(variable, value, indirect: true);
            }
        }

        private void op_inc()
        {
            using (var value = il.NewLocal<short>())
            {
                var variable = ReadByRefVariableOperand();

                ReadVariable(variable, indirect: true);
                il.ConvertToInt16();
                il.Add(1);
                value.Store();

                WriteVariable(variable, value, indirect: true);
            }
        }

        private void op_get_child()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectChildFromOperand(0);
                result.Store();

                WriteVariable(currentInstruction.StoreVariable, result);

                result.Load();
                il.LoadConstant(0);
                il.CompareGreaterThan();
                Branch();
            }
        }

        private void op_get_parent()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectParentFromOperand(0);
                result.Store();

                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_get_sibling()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectSiblingFromOperand(0);
                result.Store();

                WriteVariable(currentInstruction.StoreVariable, result);

                result.Load();
                il.LoadConstant(0);
                il.CompareGreaterThan();
                Branch();
            }
        }

        private void op_remove_obj()
        {
            using (var objNum = il.NewLocal<ushort>())
            {
                ReadOperand(0);
                objNum.Store();

                RemoveObjectFromParent(objNum);
            }
        }

        private void op_get_prop_len()
        {
            using (var dataAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<byte>())
            {
                ReadOperand(0);
                dataAddress.Store();

                var done = il.NewLabel();
                var isNotZero = il.NewLabel();

                dataAddress.Load();
                isNotZero.BranchIf(Condition.True, @short: true);

                il.LoadConstant(0);
                value.Store();
                done.Branch();

                isNotZero.Mark();

                dataAddress.Load();
                il.Subtract(1);
                il.ConvertToUInt16();
                dataAddress.Store();

                ReadByte(dataAddress);
                value.Store();

                var checkForZero = il.NewLabel();

                if (machine.Version < 4)
                {
                    value.Load();
                    il.Shr(5);
                    il.Add(1);
                    il.ConvertToUInt8();
                    value.Store();
                    checkForZero.Branch();
                }

                var secondBranch = il.NewLabel();

                value.Load();
                il.And(0x80);
                secondBranch.BranchIf(Condition.True, @short: true);

                value.Load();
                il.Shr(6);
                il.Add(1);
                il.ConvertToUInt8();
                value.Store();
                checkForZero.Branch();

                secondBranch.Mark();

                value.Load();
                il.And(0x3f);
                il.ConvertToUInt8();
                value.Store();

                checkForZero.Mark();

                value.Load();
                done.BranchIf(Condition.True, @short: true);

                il.LoadConstant(64);
                value.Store();

                done.Mark();

                WriteVariable(currentInstruction.StoreVariable, value);
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
            ReadOperand(0);
            il.LoadConstant(0);
            il.CompareEqual();

            Branch();
        }

        private void op_load()
        {
            var variable = ReadByRefVariableOperand();

            ReadVariable(variable, indirect: true);
            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_print_paddr()
        {
            il.LoadArgument(0);
            var op = GetOperand(0);
            UnpackStringAddress(op);
            il.Call(readZTextHelper);
            PrintText();
        }

        private void op_print_obj()
        {
            NotImplemented();
        }

        private void op_ret()
        {
            ReadOperand(0);
            il.Return();
        }
    }
}
