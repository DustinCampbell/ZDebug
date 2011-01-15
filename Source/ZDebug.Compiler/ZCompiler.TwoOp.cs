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
        private void BinaryOp(Instruction i, OpCode op)
        {
            ReadOperand(i.Operands[0]);
            il.Emit(OpCodes.Conv_I2);

            ReadOperand(i.Operands[1]);
            il.Emit(OpCodes.Conv_I2);

            il.Emit(op);
            il.Emit(OpCodes.Conv_U2);

            using (var temp = localManager.AllocateTemp<ushort>())
            {
                il.Emit(OpCodes.Stloc, temp);
                WriteVariable(i.StoreVariable, temp);
            }
        }

        private void op_add(Instruction i)
        {
            BinaryOp(i, OpCodes.Add);
        }

        private void op_sub(Instruction i)
        {
            BinaryOp(i, OpCodes.Sub);
        }

        private void op_mul(Instruction i)
        {
            BinaryOp(i, OpCodes.Mul);
        }

        private void op_div(Instruction i)
        {
            BinaryOp(i, OpCodes.Div);
        }

        private void op_mod(Instruction i)
        {
            BinaryOp(i, OpCodes.Rem);
        }

        private void op_insert_obj(Instruction i)
        {
            il.ThrowException("'" + i.Opcode.Name + "' not implemented.");
        }

        private void op_je(Instruction i)
        {
            using (var x = localManager.AllocateTemp<ushort>())
            using (var result = localManager.AllocateTemp<bool>())
            {
                ReadOperand(i.Operands[0]);
                il.Emit(OpCodes.Stloc, x);

                il.Emit(OpCodes.Ldc_I4_0);
                il.Emit(OpCodes.Stloc, result);

                var done = il.DefineLabel();

                for (int j = 1; j < i.OperandCount; j++)
                {
                    ReadOperand(i.Operands[j]);
                    il.Emit(OpCodes.Ldloc, x);

                    il.Emit(OpCodes.Ceq);
                    il.Emit(OpCodes.Stloc, result);

                    // no need to write a branch for the last test
                    if (j < i.OperandCount - 1)
                    {
                        il.Emit(OpCodes.Ldloc, result);
                        il.Emit(OpCodes.Brtrue_S, done);
                    }
                }

                il.MarkLabel(done);
                il.Emit(OpCodes.Ldloc, result);
                Branch(i);
            }
        }

        private void op_loadw(Instruction i)
        {
            using (var address = localManager.AllocateTemp<int>())
            using (var value = localManager.AllocateTemp<ushort>())
            {
                ReadOperand(i.Operands[0]);
                ReadOperand(i.Operands[1]);
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, address);

                ReadWord(address);
                il.Emit(OpCodes.Stloc, value);

                WriteVariable(i.StoreVariable, value);
            }
        }

        private void op_store(Instruction i)
        {
            using (var variableIndex = localManager.AllocateTemp<byte>())
            using (var value = localManager.AllocateTemp<ushort>())
            {
                ReadOperand(i.Operands[0]);
                il.Emit(OpCodes.Conv_U1);
                il.Emit(OpCodes.Stloc, variableIndex);

                ReadOperand(i.Operands[1]);
                il.Emit(OpCodes.Stloc, value);

                WriteVariable(variableIndex, value, indirect: true);
            }
        }

        private void op_test_attr(Instruction i)
        {
            il.DebugIndent();

            using (var objNum = localManager.AllocateTemp<ushort>())
            using (var attribute = localManager.AllocateTemp<byte>())
            {
                // Read objNum
                var invalidObjNum = il.DefineLabel();
                ReadValidObjectNumber(i.Operands[0], invalidObjNum);
                il.Emit(OpCodes.Stloc, objNum);

                // Read attribute
                ReadOperand(i.Operands[1]);
                il.Emit(OpCodes.Stloc, attribute);

                ObjectHasAttribute(objNum, attribute);
                Branch(i);

                var done = il.DefineLabel();
                il.Emit(OpCodes.Br_S, done);

                il.MarkLabel(invalidObjNum);
                il.LoadBool(false);
                Branch(i);

                il.MarkLabel(done);
            }

            il.DebugUnindent();
        }
    }
}
