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

        private void op_ret()
        {
            ReadOperand(0);
            il.Return();
        }
    }
}
