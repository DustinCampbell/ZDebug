using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void op_art_shift()
        {
            using (var number = il.NewLocal<short>())
            using (var places = il.NewLocal<int>())
            using (var result = il.NewLocal<ushort>())
            {
                ReadOperand(0);
                il.ConvertToInt16();
                number.Store();

                ReadOperand(1);
                il.ConvertToInt16();
                places.Store();

                var positivePlaces = il.NewLabel();
                places.Load();
                il.LoadConstant(0);
                positivePlaces.BranchIf(Condition.GreaterThan, @short: true);

                number.Load();
                places.Load();
                il.Negate();
                il.And(0x1f);
                il.Shr();
                il.ConvertToUInt16();

                var done = il.NewLabel();
                done.Branch(@short: true);

                positivePlaces.Mark();

                number.Load();
                places.Load();
                il.And(0x1f);
                il.Shl();
                il.ConvertToUInt16();

                done.Mark();

                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_log_shift()
        {
            using (var number = il.NewLocal<ushort>())
            using (var places = il.NewLocal<int>())
            using (var result = il.NewLocal<ushort>())
            {
                ReadOperand(0);
                number.Store();

                ReadOperand(1);
                il.ConvertToInt16();
                places.Store();

                var positivePlaces = il.NewLabel();
                places.Load();
                il.LoadConstant(0);
                positivePlaces.BranchIf(Condition.GreaterThan, @short: true);

                number.Load();
                places.Load();
                il.Negate();
                il.And(0x1f);
                il.Shr();
                il.ConvertToUInt16();

                var done = il.NewLabel();
                done.Branch(@short: true);

                positivePlaces.Mark();

                number.Load();
                places.Load();
                il.And(0x1f);
                il.Shl();
                il.ConvertToUInt16();

                done.Mark();

                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }
    }
}
