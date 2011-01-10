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
