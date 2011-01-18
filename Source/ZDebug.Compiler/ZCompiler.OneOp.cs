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

        private void op_print_paddr()
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
