using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;
using System.Reflection.Emit;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void BinaryOp(OpCode op, bool signed = false)
        {
            ReadOperand(0);
            if (signed)
            {
                il.Emit(OpCodes.Conv_I2);
            }

            ReadOperand(1);
            if (signed)
            {
                il.Emit(OpCodes.Conv_I2);
            }

            il.Emit(op);
            il.Emit(OpCodes.Conv_U2);

            using (var temp = localManager.AllocateTemp<ushort>())
            {
                il.Emit(OpCodes.Stloc, temp);
                WriteVariable(currentInstruction.StoreVariable, temp);
            }
        }

        private void op_add()
        {
            BinaryOp(OpCodes.Add, signed: true);
        }

        private void op_sub()
        {
            BinaryOp(OpCodes.Sub, signed: true);
        }

        private void op_mul()
        {
            BinaryOp(OpCodes.Mul, signed: true);
        }

        private void op_div()
        {
            BinaryOp(OpCodes.Div, signed: true);
        }

        private void op_mod()
        {
            BinaryOp(OpCodes.Rem, signed: true);
        }

        private void op_or()
        {
            BinaryOp(OpCodes.Or, signed: false);
        }

        private void op_and()
        {
            BinaryOp(OpCodes.And, signed: false);
        }

        private void op_dec_chk()
        {
            using (var variableIndex = localManager.AllocateTemp<byte>())
            using (var value = localManager.AllocateTemp<short>())
            {
                ReadOperand(0);
                il.Emit(OpCodes.Conv_U1);
                il.Emit(OpCodes.Stloc, variableIndex);

                ReadVariable(variableIndex, indirect: true);
                il.Emit(OpCodes.Conv_I2);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Sub);
                il.Emit(OpCodes.Stloc, value);

                WriteVariable(variableIndex, value, indirect: true);

                il.Emit(OpCodes.Ldloc, value);
                ReadOperand(1);
                il.Emit(OpCodes.Conv_I2);

                il.Emit(OpCodes.Clt);
                Branch();
            }
        }

        private void op_inc_chk()
        {
            using (var variableIndex = localManager.AllocateTemp<byte>())
            using (var value = localManager.AllocateTemp<short>())
            {
                ReadOperand(0);
                il.Emit(OpCodes.Conv_U1);
                il.Emit(OpCodes.Stloc, variableIndex);

                ReadVariable(variableIndex, indirect: true);
                il.Emit(OpCodes.Conv_I2);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, value);

                WriteVariable(variableIndex, value, indirect: true);

                il.Emit(OpCodes.Ldloc, value);
                ReadOperand(1);
                il.Emit(OpCodes.Conv_I2);

                il.Emit(OpCodes.Cgt);
                Branch();
            }
        }

        private void op_insert_obj()
        {
            NotImplemented();
        }

        private void op_je()
        {
            using (var x = localManager.AllocateTemp<ushort>())
            using (var result = localManager.AllocateTemp<bool>())
            {
                ReadOperand(0);
                il.Emit(OpCodes.Stloc, x);

                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, result);

                var done = il.DefineLabel();

                for (int j = 1; j < currentInstruction.OperandCount; j++)
                {
                    ReadOperand(j);
                    il.Emit(OpCodes.Ldloc, x);

                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Stloc, result);

                    // no need to write a branch for the last test
                    if (j < currentInstruction.OperandCount - 1)
                    {
                        il.Emit(OpCodes.Ldloc, result);
                        il.Emit(OpCodes.Brtrue_S, done);
                    }
                }

                il.MarkLabel(done);
                il.Emit(OpCodes.Ldloc, result);
                Branch();
            }
        }

        private void op_loadb()
        {
            using (var address = localManager.AllocateTemp<int>())
            using (var value = localManager.AllocateTemp<ushort>())
            {
                ReadOperand(0);
                ReadOperand(1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, address);

                ReadByte(address);
                il.Emit(OpCodes.Stloc, value);

                WriteVariable(currentInstruction.StoreVariable, value);
            }
        }

        private void op_loadw()
        {
            using (var address = localManager.AllocateTemp<int>())
            using (var value = localManager.AllocateTemp<ushort>())
            {
                ReadOperand(0);
                ReadOperand(1);
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, address);

                ReadWord(address);
                il.Emit(OpCodes.Stloc, value);

                WriteVariable(currentInstruction.StoreVariable, value);
            }
        }

        private void op_store()
        {
            using (var variableIndex = localManager.AllocateTemp<byte>())
            using (var value = localManager.AllocateTemp<ushort>())
            {
                ReadOperand(0);
                il.Emit(OpCodes.Conv_U1);
                il.Emit(OpCodes.Stloc, variableIndex);

                ReadOperand(1);
                il.Emit(OpCodes.Stloc, value);

                WriteVariable(variableIndex, value, indirect: true);
            }
        }

        private void op_test_attr()
        {
            using (var objNum = localManager.AllocateTemp<ushort>())
            using (var attribute = localManager.AllocateTemp<byte>())
            {
                // Read objNum
                var invalidObjNum = il.DefineLabel();
                ReadValidObjectNumber(0, invalidObjNum);
                il.Emit(OpCodes.Stloc, objNum);

                // Read attribute
                ReadOperand(1);
                il.Emit(OpCodes.Stloc, attribute);

                ObjectHasAttribute(objNum, attribute);
                Branch();

                var done = il.DefineLabel();
                il.Emit(OpCodes.Br_S, done);

                il.MarkLabel(invalidObjNum);
                il.LoadBool(false);
                Branch();

                il.MarkLabel(done);
            }
        }
    }
}
