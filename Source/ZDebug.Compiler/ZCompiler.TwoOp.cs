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
        private void BinaryOp(CodeBuilder operation, bool signed = false)
        {
            ReadOperand(0);
            if (signed)
            {
                il.ConvertToInt16();
            }

            ReadOperand(1);
            if (signed)
            {
                il.ConvertToInt16();
            }

            operation();
            il.ConvertToUInt16();

            using (var temp = il.NewLocal<ushort>())
            {
                temp.Store();
                WriteVariable(currentInstruction.StoreVariable, temp);
            }
        }

        private void op_add()
        {
            BinaryOp(il.GenerateAdd(), signed: true);
        }

        private void op_sub()
        {
            BinaryOp(il.GenerateSubtract(), signed: true);
        }

        private void op_mul()
        {
            BinaryOp(il.GenerateMultiply(), signed: true);
        }

        private void op_div()
        {
            BinaryOp(il.GenerateDivide(), signed: true);
        }

        private void op_mod()
        {
            BinaryOp(il.GenerateRemainder(), signed: true);
        }

        private void op_or()
        {
            BinaryOp(il.GenerateOr(), signed: false);
        }

        private void op_and()
        {
            BinaryOp(il.GenerateAnd(), signed: false);
        }

        private void op_dec_chk()
        {
            using (var value = il.NewLocal<short>())
            {
                var variable = ReadByRefVariableOperand();

                ReadVariable(variable, indirect: true);
                il.ConvertToInt16();
                il.Subtract(1);
                value.Store();

                WriteVariable(variable, value, indirect: true);

                value.Load();
                ReadOperand(1);
                il.ConvertToInt16();

                il.CompareLessThan();
                Branch();
            }
        }

        private void op_inc_chk()
        {
            using (var value = il.NewLocal<short>())
            {
                var variable = ReadByRefVariableOperand();

                ReadVariable(variable, indirect: true);
                il.ConvertToInt16();
                il.Add(1);
                value.Store();

                WriteVariable(variable, value, indirect: true);

                value.Load();
                ReadOperand(1);
                il.ConvertToInt16();

                il.CompareGreaterThan();
                Branch();
            }
        }

        private void op_insert_obj()
        {
            NotImplemented();
        }

        private void op_je()
        {
            using (var x = il.NewLocal<ushort>())
            using (var result = il.NewLocal<bool>())
            {
                ReadOperand(0);
                x.Store();

                il.LoadConstant(0);
                result.Store();

                var done = il.NewLabel();

                for (int j = 1; j < currentInstruction.OperandCount; j++)
                {
                    ReadOperand(j);
                    x.Load();

                    il.CompareEqual();
                    result.Store();

                    // no need to write a branch for the last test
                    if (j < currentInstruction.OperandCount - 1)
                    {
                        result.Load();
                        done.BranchIf(Condition.True, @short: true);
                    }
                }

                done.Mark();
                result.Load();
                Branch();
            }
        }

        private void op_loadb()
        {
            using (var address = il.NewLocal<int>())
            using (var value = il.NewLocal<ushort>())
            {
                ReadOperand(0);
                ReadOperand(1);
                il.Add();
                address.Store();

                ReadByte(address);
                value.Store();

                WriteVariable(currentInstruction.StoreVariable, value);
            }
        }

        private void op_loadw()
        {
            using (var address = il.NewLocal<int>())
            using (var value = il.NewLocal<ushort>())
            {
                ReadOperand(0);
                ReadOperand(1);
                il.Multiply(2);
                il.Add();
                address.Store();

                ReadWord(address);
                value.Store();

                WriteVariable(currentInstruction.StoreVariable, value);
            }
        }

        private void op_store()
        {
            NotImplemented();
            return;

            using (var value = il.NewLocal<ushort>())
            {
                var variable = ReadByRefVariableOperand();

                ReadOperand(1);
                value.Store();

                WriteVariable(variable, value, indirect: true);
            }
        }

        private void op_test_attr()
        {
            NotImplemented();
            return;

            using (var objNum = il.NewLocal<ushort>())
            using (var attribute = il.NewLocal<byte>())
            {
                // Read objNum
                var invalidObjNum = il.NewLabel();
                ReadValidObjectNumber(0, invalidObjNum);
                objNum.Store();

                // Read attribute
                ReadOperand(1);
                attribute.Store();

                ObjectHasAttribute(objNum, attribute);
                Branch();

                var done = il.NewLabel();
                done.Branch(@short: true);

                invalidObjNum.Mark();
                il.LoadConstant(false);
                Branch();

                done.Mark();
            }
        }
    }
}
