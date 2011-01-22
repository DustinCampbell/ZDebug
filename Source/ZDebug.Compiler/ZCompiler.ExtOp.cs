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
                il.Convert.ToInt16();
                number.Store();

                ReadOperand(1);
                il.Convert.ToInt16();
                places.Store();

                var positivePlaces = il.NewLabel();
                places.Load();
                il.Load(0);
                positivePlaces.BranchIf(Condition.GreaterThan, @short: true);

                number.Load();
                places.Load();
                il.Math.Negate();
                il.Math.And(0x1f);
                il.Math.Shr();
                il.Convert.ToUInt16();

                var done = il.NewLabel();
                done.Branch(@short: true);

                positivePlaces.Mark();

                number.Load();
                places.Load();
                il.Math.And(0x1f);
                il.Math.Shl();
                il.Convert.ToUInt16();

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
                il.Convert.ToInt16();
                places.Store();

                var positivePlaces = il.NewLabel();
                places.Load();
                il.Load(0);
                positivePlaces.BranchIf(Condition.GreaterThan, @short: true);

                number.Load();
                places.Load();
                il.Math.Negate();
                il.Math.And(0x1f);
                il.Math.Shr();
                il.Convert.ToUInt16();

                var done = il.NewLabel();
                done.Branch(@short: true);

                positivePlaces.Mark();

                number.Load();
                places.Load();
                il.Math.And(0x1f);
                il.Math.Shl();
                il.Convert.ToUInt16();

                done.Mark();

                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_save_undo()
        {
            NotImplemented();
        }

        private void op_restore_undo()
        {
            NotImplemented();
        }
    }
}
