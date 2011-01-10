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
        private void op_call_s(Instruction i)
        {
            // TODO: Can we do better here? Allocating a new array to hold
            // every call's arguments might be expensive. We should share
            // this at some point in the future.

            using (var address = localManager.AllocateTemp<int>())
            using (var args = localManager.AllocateTemp<ushort[]>())
            using (var result = localManager.AllocateTemp<ushort>())
            {
                UnpackRoutineAddress(i.Operands[0]);
                il.Emit(OpCodes.Stloc, address);

                il.Emit(OpCodes.Ldc_I4, i.OperandCount - 1);
                il.Emit(OpCodes.Newarr, typeof(ushort));
                il.Emit(OpCodes.Stloc, args);

                for (int j = 1; j < i.OperandCount; j++)
                {
                    il.Emit(OpCodes.Ldloc, args);
                    il.Emit(OpCodes.Ldc_I4, j - 1);
                    ReadOperand(i.Operands[j]);
                    il.Emit(OpCodes.Stelem_I2);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc, address);
                il.Emit(OpCodes.Ldloc, args);

                il.Emit(OpCodes.Call, callHelper);

                il.Emit(OpCodes.Stloc, result);
                WriteVariable(i.StoreVariable, result);
            }
        }

        private void op_put_prop(Instruction i)
        {
            il.ThrowException("'" + i.Opcode.Name + "' not implemented.");
        }

        private void op_storew(Instruction i)
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

                ReadOperand(i.Operands[2]);
                il.Emit(OpCodes.Stloc, value);

                WriteWord(address, value);
            }
        }
    }
}
