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
        private void op_jump(Instruction i)
        {
            var address = i.Address + i.Length + (short)(i.Operands[0].Value) - 2;
            var jump = addressToLabelMap[address];
            il.Emit(OpCodes.Br, jump);
        }

        private void op_jz(Instruction i)
        {
            ReadOperand(i.Operands[0]);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);

            Branch(i);
        }

        private void op_print_paddr(Instruction i)
        {
            il.ThrowException("'" + i.Opcode.Name + "' not implemented.");
        }

        private void op_ret(Instruction i)
        {
            ReadOperand(i.Operands[0]);
            il.Emit(OpCodes.Ret);
        }
    }
}
