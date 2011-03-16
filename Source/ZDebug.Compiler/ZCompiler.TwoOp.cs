using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void BinaryOp(CodeBuilder operation, bool signed = false)
        {
            LoadOperand(0);
            if (signed)
            {
                il.Convert.ToInt16();
            }

            LoadOperand(1);
            if (signed)
            {
                il.Convert.ToInt16();
            }

            operation();
            il.Convert.ToUInt16();

            using (var temp = il.NewLocal<ushort>())
            {
                temp.Store();
                StoreVariable(currentInstruction.StoreVariable, temp);
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
            using (var varIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<short>())
            {
                LoadByRefVariableOperand();
                varIndex.Store();

                LoadVariable(varIndex, indirect: true);
                il.Convert.ToInt16();
                il.Math.Subtract(1);
                value.Store();

                StoreVariable(varIndex, value, indirect: true);

                value.Load();
                LoadOperand(1);
                il.Convert.ToInt16();

                il.Compare.LessThan();
                Branch();
            }
        }

        private void op_inc_chk()
        {
            using (var varIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<short>())
            {
                LoadByRefVariableOperand();
                varIndex.Store();

                LoadVariable(varIndex, indirect: true);
                il.Convert.ToInt16();
                il.Math.Add(1);
                value.Store();

                StoreVariable(varIndex, value, indirect: true);

                value.Load();
                LoadOperand(1);
                il.Convert.ToInt16();

                il.Compare.GreaterThan();
                Branch();
            }
        }

        private void op_insert_obj()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var destNum = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                ReadValidObjectNumber(0, done);
                objNum.Store();

                ReadValidObjectNumber(1, done);
                destNum.Store();

                MoveObjectToDestination(objNum, destNum);

                done.Mark();
            }
        }

        private void op_je()
        {
            using (var x = il.NewLocal<ushort>())
            {
                LoadOperand(0);
                x.Store();

                var success = il.NewLabel();
                var done = il.NewLabel();

                for (int j = 1; j < currentInstruction.OperandCount; j++)
                {
                    LoadOperand(j);
                    x.Load();

                    il.Compare.Equal();

                    // no need to write a branch for the last test
                    if (j < currentInstruction.OperandCount - 1)
                    {
                        success.BranchIf(Condition.True, @short: true);
                    }
                    else
                    {
                        done.Branch(@short: true);
                    }
                }

                success.Mark();
                il.Load(1);

                done.Mark();
                Branch();
            }
        }

        private void op_jg()
        {
            LoadOperand(0);
            il.Convert.ToInt16();

            LoadOperand(1);
            il.Convert.ToInt16();

            il.Compare.GreaterThan();
            Branch();
        }

        private void op_jl()
        {
            LoadOperand(0);
            il.Convert.ToInt16();

            LoadOperand(1);
            il.Convert.ToInt16();

            il.Compare.LessThan();
            Branch();
        }

        private void op_jin()
        {
            ReadObjectParentFromOperand(0);
            LoadOperand(1);

            il.Compare.Equal();
            Branch();
        }

        private void op_test()
        {
            using (var flags = il.NewLocal<ushort>())
            {
                LoadOperand(1);
                flags.Store();

                LoadOperand(0);
                flags.Load();
                il.Math.And();

                flags.Load();

                il.Compare.Equal();
                Branch();
            }
        }

        private void op_loadb()
        {
            var addressOp = GetOperand(0);
            var offsetOp = GetOperand(1);

            // Are the address and offset operands constants? If so, we can fold them at compile time.
            if (addressOp.Kind != OperandKind.Variable &&
                offsetOp.Kind != OperandKind.Variable)
            {
                StoreVariable(currentInstruction.StoreVariable,
                    valueLoader: () =>
                    {
                        var address = addressOp.Value + offsetOp.Value;
                        LoadByte(address);
                    });
            }
            else
            {
                using (var address = il.NewLocal<int>())
                {
                    LoadOperand(0);
                    LoadOperand(1);
                    il.Math.Add();
                    address.Store();

                    StoreVariable(currentInstruction.StoreVariable,
                        valueLoader: () =>
                        {
                            LoadByte(address);
                        });
                }
            }
        }

        private void op_loadw()
        {
            var addressOp = GetOperand(0);
            var offsetOp = GetOperand(1);

            // Are the address and offset operands constants? If so, we can fold them at compile time.
            if (addressOp.Kind != OperandKind.Variable &&
                offsetOp.Kind != OperandKind.Variable)
            {
                StoreVariable(currentInstruction.StoreVariable,
                    valueLoader: () =>
                    {
                        var address = addressOp.Value + (offsetOp.Value * 2);
                        LoadWord(address);
                    });
            }
            else
            {
                using (var address = il.NewLocal<int>())
                {
                    LoadOperand(0);
                    LoadOperand(1);
                    il.Math.Multiply(2);
                    il.Math.Add();
                    address.Store();

                    StoreVariable(currentInstruction.StoreVariable,
                        valueLoader: () =>
                        {
                            LoadWord(address);
                        });
                }
            }
        }

        private void op_store()
        {
            using (var varIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<ushort>())
            {
                LoadByRefVariableOperand();
                varIndex.Store();

                LoadOperand(1);
                value.Store();

                StoreVariable(varIndex, value, indirect: true);
            }
        }

        private void op_get_prop()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var propNum = il.NewLocal<ushort>())
            using (var propAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<ushort>())
            using (var result = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                // Read objNum
                var invalidObjNum = il.NewLabel();
                ReadValidObjectNumber(0, invalidObjNum);
                objNum.Store();

                // Read propNum
                LoadOperand(1);
                propNum.Store();

                int mask = machine.Version < 4 ? 0x1f : 0x3f;

                // Read first property address into propAddress
                FirstProperty(objNum);
                propAddress.Store();

                var loopStart = il.NewLabel();
                var loopDone = il.NewLabel();

                loopStart.Mark();

                LoadByte(propAddress);
                value.Store();

                value.Load();
                il.Math.And(mask);
                il.Convert.ToUInt16();
                propNum.Load();
                loopDone.BranchIf(Condition.AtMost, @short: true);

                propAddress.Load();
                NextProperty();
                propAddress.Store();

                loopStart.Branch();

                loopDone.Mark();

                var propNotFound = il.NewLabel();

                value.Load();
                il.Math.And(mask);
                propNum.Load();
                propNotFound.BranchIf(Condition.NotEqual);

                propAddress.Load();
                il.Math.Add(1);
                il.Convert.ToUInt16();
                propAddress.Store();

                var sizeMask = machine.Version < 4 ? 0xe0 : 0xc0;

                var secondBranch = il.NewLabel();

                value.Load();
                il.Math.And(sizeMask);
                secondBranch.BranchIf(Condition.True, @short: true);

                LoadByte(propAddress);
                result.Store();

                done.Branch();

                secondBranch.Mark();

                LoadWord(propAddress);
                result.Store();

                done.Branch();

                propNotFound.Mark();

                propNum.Load();
                il.Math.Subtract(1);
                il.Math.Multiply(2);
                il.Math.Add(machine.ObjectTableAddress);
                il.Convert.ToUInt16();
                propAddress.Store();

                LoadWord(propAddress);
                result.Store();

                done.Branch();

                invalidObjNum.Mark();

                il.Load(0);
                result.Store();

                done.Mark();

                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_get_next_prop()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var propNum = il.NewLocal<ushort>())
            using (var propAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                // Read objNum
                var invalidObjNum = il.NewLabel();
                ReadValidObjectNumber(0, invalidObjNum);
                objNum.Store();

                // Read propNum
                LoadOperand(1);
                propNum.Store();

                int mask = machine.Version < 4 ? 0x1f : 0x3f;

                // Read first property address into propAddress
                FirstProperty(objNum);
                propAddress.Store();

                var storePropNum = il.NewLabel();

                // propNum == 0?
                propNum.Load();
                storePropNum.BranchIf(Condition.False);

                var loopStart = il.NewLabel();

                // start of the loop
                loopStart.Mark();

                LoadByte(propAddress);
                value.Store();

                propAddress.Load();
                NextProperty();
                propAddress.Store();

                value.Load();
                il.Load(mask);
                il.Math.And();
                il.Convert.ToUInt16();
                propNum.Load();
                loopStart.BranchIf(Condition.GreaterThan);

                // loop complete - check if propNum and value match.
                value.Load();
                il.Load(mask);
                il.Math.And();
                il.Convert.ToUInt16();
                propNum.Load();
                storePropNum.BranchIf(Condition.Equal, @short: true);

                il.RuntimeError("Could not find property {0} on object {1}.", propNum, objNum);

                // At this point, we're done - store the value
                storePropNum.Mark();

                LoadByte(propAddress);
                il.Load(mask);
                il.Math.And();
                il.Convert.ToUInt16();
                value.Store();
                StoreVariable(currentInstruction.StoreVariable, value);

                done.Branch(@short: true);

                // invalid object encountered
                invalidObjNum.Mark();

                il.Load(0);
                value.Store();
                StoreVariable(currentInstruction.StoreVariable, value);

                done.Mark();
            }
        }

        private void op_get_prop_addr()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var propNum = il.NewLocal<ushort>())
            using (var propAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                // Read objNum
                var storeZero = il.NewLabel();
                ReadValidObjectNumber(0, storeZero);
                objNum.Store();

                // Read propNum
                LoadOperand(1);
                propNum.Store();

                int mask = machine.Version < 4 ? 0x1f : 0x3f;

                // Read first property address into propAddress
                FirstProperty(objNum);
                propAddress.Store();

                var loopStart = il.NewLabel();
                var loopDone = il.NewLabel();

                loopStart.Mark();

                LoadByte(propAddress);
                value.Store();

                value.Load();
                il.Math.And(mask);
                il.Convert.ToUInt16();
                propNum.Load();
                loopDone.BranchIf(Condition.AtMost, @short: true);

                propAddress.Load();
                NextProperty();
                propAddress.Store();

                loopStart.Branch();

                loopDone.Mark();

                var storeAddress = il.NewLabel();

                value.Load();
                il.Math.And(mask);
                propNum.Load();
                storeZero.BranchIf(Condition.NotEqual, @short: true);

                if (machine.Version > 3)
                {
                    value.Load();
                    il.Math.And(0x80);
                    storeAddress.BranchIf(Condition.False, @short: true);

                    propAddress.Load();
                    il.Math.Add(1);
                    il.Convert.ToUInt16();
                    propAddress.Store();
                }

                storeAddress.Mark();

                propAddress.Load();
                il.Math.Add(1);
                il.Convert.ToUInt16();
                propAddress.Store();
                StoreVariable(currentInstruction.StoreVariable, propAddress);

                done.Branch(@short: true);

                storeZero.Mark();

                il.Load(0);
                value.Store();
                StoreVariable(currentInstruction.StoreVariable, value);

                done.Mark();
            }
        }

        private void op_test_attr()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var attribute = il.NewLocal<byte>())
            {
                // Read objNum
                var invalidObjNum = il.NewLabel();
                ReadValidObjectNumber(0, invalidObjNum);
                objNum.Store();

                // Read attribute
                LoadOperand(1);
                attribute.Store();

                ObjectHasAttribute(objNum, attribute);
                Branch();

                var done = il.NewLabel();
                done.Branch(@short: true);

                invalidObjNum.Mark();
                il.Load(false);
                Branch();

                done.Mark();
            }
        }

        private void op_set_attr()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var attribute = il.NewLocal<byte>())
            {
                // Read objNum
                var invalidObjNum = il.NewLabel();
                ReadValidObjectNumber(0, invalidObjNum);
                objNum.Store();

                // Read attribute
                LoadOperand(1);
                attribute.Store();

                ObjectSetAttribute(objNum, attribute, true);

                invalidObjNum.Mark();
            }
        }

        private void op_clear_attr()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var attribute = il.NewLocal<byte>())
            {
                // Read objNum
                var invalidObjNum = il.NewLabel();
                ReadValidObjectNumber(0, invalidObjNum);
                objNum.Store();

                // Read attribute
                LoadOperand(1);
                attribute.Store();

                ObjectSetAttribute(objNum, attribute, false);

                invalidObjNum.Mark();
            }
        }
    }
}
