using ZDebug.Compiler.Generate;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        // Arithmetic routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void BinaryOp(CodeBuilder operation, bool signed = false)
        {
            using (var temp = il.NewLocal<ushort>())
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

                temp.Store();
                StoreVariable(currentInstruction.StoreVariable, temp);
            }
        }

        private void op_add()
        {
            BinaryOp(il.GenerateAdd(), signed: true);
        }

        private void op_div()
        {
            BinaryOp(il.GenerateDivide(), signed: true);
        }

        private void op_mod()
        {
            BinaryOp(il.GenerateRemainder(), signed: true);
        }

        private void op_mul()
        {
            BinaryOp(il.GenerateMultiply(), signed: true);
        }

        private void op_sub()
        {
            BinaryOp(il.GenerateSubtract(), signed: true);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Bit-level routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_and()
        {
            BinaryOp(il.GenerateAnd(), signed: false);
        }

        private void op_art_shift()
        {
            using (var number = il.NewLocal<short>())
            using (var places = il.NewLocal<int>())
            using (var result = il.NewLocal<ushort>())
            {
                LoadOperand(0);
                il.Convert.ToInt16();
                number.Store();

                LoadOperand(1);
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
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_log_shift()
        {
            using (var number = il.NewLocal<ushort>())
            using (var places = il.NewLocal<int>())
            using (var result = il.NewLocal<ushort>())
            {
                LoadOperand(0);
                number.Store();

                LoadOperand(1);
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
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_or()
        {
            BinaryOp(il.GenerateOr(), signed: false);
        }

        private void op_not()
        {
            LoadOperand(0);
            il.Math.Not();
            il.Convert.ToUInt16();

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
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

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Increment/decrement routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_dec()
        {
            var varIndexOp = GetOperand(0);

            if (varIndexOp.Kind == OperandKind.SmallConstant)
            {
                using (var value = il.NewLocal<short>())
                {
                    var varIndex = (byte)varIndexOp.Value;

                    LoadVariable(varIndex, indirect: true);
                    il.Convert.ToInt16();
                    il.Math.Subtract(1);
                    value.Store();

                    StoreVariable(varIndex, value, indirect: true);
                }
            }
            else if (varIndexOp.Kind == OperandKind.Variable)
            {
                using (var varIndex = il.NewLocal<byte>())
                using (var value = il.NewLocal<short>())
                {
                    LoadVariable((byte)varIndexOp.Value);
                    varIndex.Store();

                    CalculatedLoadVariable(varIndex, indirect: true);
                    il.Convert.ToInt16();
                    il.Math.Subtract(1);
                    value.Store();

                    CalculatedStoreVariable(varIndex, value, indirect: true);
                }
            }
            else
            {
                throw new ZCompilerException("Expected small constant or variable, but was " + varIndexOp.Kind);
            }
        }

        private void op_dec_chk()
        {
            var varIndexOp = GetOperand(0);

            if (varIndexOp.Kind == OperandKind.SmallConstant)
            {
                using (var value = il.NewLocal<short>())
                {
                    var varIndex = (byte)varIndexOp.Value;

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
            else if (varIndexOp.Kind == OperandKind.Variable)
            {
                using (var varIndex = il.NewLocal<byte>())
                using (var value = il.NewLocal<short>())
                {
                    LoadVariable((byte)varIndexOp.Value);
                    varIndex.Store();

                    CalculatedLoadVariable(varIndex, indirect: true);
                    il.Convert.ToInt16();
                    il.Math.Subtract(1);
                    value.Store();

                    CalculatedStoreVariable(varIndex, value, indirect: true);

                    value.Load();
                    LoadOperand(1);
                    il.Convert.ToInt16();

                    il.Compare.LessThan();
                    Branch();
                }
            }
            else
            {
                throw new ZCompilerException("Expected small constant or variable, but was " + varIndexOp.Kind);
            }
        }

        private void op_inc()
        {
            var varIndexOp = GetOperand(0);

            if (varIndexOp.Kind == OperandKind.SmallConstant)
            {
                using (var value = il.NewLocal<short>())
                {
                    var varIndex = (byte)varIndexOp.Value;

                    LoadVariable(varIndex, indirect: true);
                    il.Convert.ToInt16();
                    il.Math.Add(1);
                    value.Store();

                    StoreVariable(varIndex, value, indirect: true);
                }
            }
            else if (varIndexOp.Kind == OperandKind.Variable)
            {
                using (var varIndex = il.NewLocal<byte>())
                using (var value = il.NewLocal<short>())
                {
                    LoadVariable((byte)varIndexOp.Value);
                    varIndex.Store();

                    CalculatedLoadVariable(varIndex, indirect: true);
                    il.Convert.ToInt16();
                    il.Math.Add(1);
                    value.Store();

                    CalculatedStoreVariable(varIndex, value, indirect: true);
                }
            }
            else
            {
                throw new ZCompilerException("Expected small constant or variable, but was " + varIndexOp.Kind);
            }
        }

        private void op_inc_chk()
        {
            var varIndexOp = GetOperand(0);

            if (varIndexOp.Kind == OperandKind.SmallConstant)
            {
                using (var value = il.NewLocal<short>())
                {
                    var varIndex = (byte)varIndexOp.Value;

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
            else if (varIndexOp.Kind == OperandKind.Variable)
            {
                using (var varIndex = il.NewLocal<byte>())
                using (var value = il.NewLocal<short>())
                {
                    LoadVariable((byte)varIndexOp.Value);
                    varIndex.Store();

                    CalculatedLoadVariable(varIndex, indirect: true);
                    il.Convert.ToInt16();
                    il.Math.Add(1);
                    value.Store();

                    CalculatedStoreVariable(varIndex, value, indirect: true);

                    value.Load();
                    LoadOperand(1);
                    il.Convert.ToInt16();

                    il.Compare.GreaterThan();
                    Branch();
                }
            }
            else
            {
                throw new ZCompilerException("Expected small constant or variable, but was " + varIndexOp.Kind);
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Jump routines
        ///////////////////////////////////////////////////////////////////////////////////////////

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

        private void op_jin()
        {
            ReadObjectParentFromOperand(0);
            LoadOperand(1);

            il.Compare.Equal();
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

        private void op_jump()
        {
            var address = currentInstruction.Address + currentInstruction.Length + (short)(currentInstruction.Operands[0].Value) - 2;
            var jump = addressToLabelMap[address];
            jump.Branch();
        }

        private void op_jz()
        {
            LoadOperand(0);
            il.Load(0);
            il.Compare.Equal();

            Branch();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Call routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_call_n()
        {
            il.DebugIndent();

            Call();

            // discard result...
            il.Pop();

            il.DebugUnindent();
        }

        private void op_call_s()
        {
            il.DebugIndent();

            using (var result = il.NewLocal<ushort>())
            {
                Call();

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }

            il.DebugUnindent();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Return routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_ret()
        {
            LoadOperand(0);
            Return();
        }

        private void op_ret_popped()
        {
            PopStack();
            Return();
        }

        private void op_rfalse()
        {
            Return(0);
        }

        private void op_rtrue()
        {
            Return(1);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Load/Store routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_load()
        {
            var varIndexOp = GetOperand(0);

            if (varIndexOp.Kind == OperandKind.SmallConstant)
            {
                using (var result = il.NewLocal<short>())
                {
                    var varIndex = (byte)varIndexOp.Value;

                    LoadVariable(varIndex, indirect: true);
                    result.Store();

                    StoreVariable(currentInstruction.StoreVariable, result);
                }
            }
            else if (varIndexOp.Kind == OperandKind.Variable)
            {
                using (var varIndex = il.NewLocal<byte>())
                using (var result = il.NewLocal<ushort>())
                {
                    LoadVariable((byte)varIndexOp.Value);
                    varIndex.Store();

                    CalculatedLoadVariable(varIndex, indirect: true);
                    result.Store();
                    StoreVariable(currentInstruction.StoreVariable, result);
                }
            }
            else
            {
                throw new ZCompilerException("Expected small constant or variable, but was " + varIndexOp.Kind);
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
            var varIndexOp = GetOperand(0);

            if (varIndexOp.Kind == OperandKind.SmallConstant)
            {
                using (var value = il.NewLocal<short>())
                {
                    var varIndex = (byte)varIndexOp.Value;

                    LoadOperand(1);
                    value.Store();

                    StoreVariable(varIndex, value, indirect: true);
                }
            }
            else if (varIndexOp.Kind == OperandKind.Variable)
            {
                using (var varIndex = il.NewLocal<byte>())
                using (var value = il.NewLocal<ushort>())
                {
                    LoadVariable((byte)varIndexOp.Value);
                    varIndex.Store();

                    LoadOperand(1);
                    value.Store();

                    CalculatedStoreVariable(varIndex, value, indirect: true);
                }
            }
            else
            {
                throw new ZCompilerException("Expected small constant or variable, but was " + varIndexOp.Kind);
            }
        }

        private void op_storeb()
        {
            var addressOp = GetOperand(0);
            var offsetOp = GetOperand(1);

            // Are the address and offset operands constants? If so, we can fold them at compile time.
            if (addressOp.Kind != OperandKind.Variable &&
                offsetOp.Kind != OperandKind.Variable)
            {
                StoreByte(
                    address: addressOp.Value + offsetOp.Value,
                    valueLoader: () =>
                    {
                        LoadOperand(2);
                        il.Convert.ToUInt8();
                    });
            }
            else
            {
                using (var address = il.NewLocal<int>())
                using (var value = il.NewLocal<byte>())
                {
                    LoadOperand(0);
                    LoadOperand(1);
                    il.Math.Add();
                    address.Store();

                    LoadOperand(2);
                    il.Convert.ToUInt8();
                    value.Store();

                    StoreByte(address, value);
                }
            }
        }

        private void op_storew()
        {
            var addressOp = GetOperand(0);
            var offsetOp = GetOperand(1);

            // Are the address and offset operands constants? If so, we can fold them at compile time.
            if (addressOp.Kind != OperandKind.Variable &&
                offsetOp.Kind != OperandKind.Variable)
            {
                StoreWord(
                    address: addressOp.Value + (offsetOp.Value * 2),
                    valueLoader: () => LoadOperand(2));
            }
            else
            {
                using (var address = il.NewLocal<int>())
                using (var value = il.NewLocal<ushort>())
                {
                    LoadOperand(0);
                    LoadOperand(1);
                    il.Math.Multiply(2);
                    il.Math.Add();
                    address.Store();

                    LoadOperand(2);
                    value.Store();

                    StoreWord(address, value);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Table routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Stack routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_pull()
        {
            var varIndexOp = GetOperand(0);

            if (varIndexOp.Kind == OperandKind.SmallConstant)
            {
                using (var value = il.NewLocal<short>())
                {
                    var varIndex = (byte)varIndexOp.Value;

                    PopStack();
                    value.Store();

                    StoreVariable(varIndex, value, indirect: true);
                }
            }
            else if (varIndexOp.Kind == OperandKind.Variable)
            {
                using (var varIndex = il.NewLocal<byte>())
                using (var value = il.NewLocal<ushort>())
                {
                    LoadByRefVariableOperand();
                    varIndex.Store();

                    PopStack();

                    value.Store();
                    CalculatedStoreVariable(varIndex, value, indirect: true);
                }
            }
            else
            {
                throw new ZCompilerException("Expected small constant or variable, but was " + varIndexOp.Kind);
            }
        }

        private void op_push()
        {
            LoadOperand(0);
            PushStack();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Object routines
        ///////////////////////////////////////////////////////////////////////////////////////////

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

        private void op_get_child()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectChildFromOperand(0);
                result.Store();

                StoreVariable(currentInstruction.StoreVariable, result);

                result.Load();
                il.Load(0);
                il.Compare.GreaterThan();
                Branch();
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

        private void op_get_parent()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectParentFromOperand(0);
                result.Store();

                StoreVariable(currentInstruction.StoreVariable, result);
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
                ReadValidObjectNumber(operandIndex: 0, failed: storeZero);
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

                StoreVariable(currentInstruction.StoreVariable,
                    valueLoader: () =>
                    {
                        propAddress.Load();
                        il.Math.Add(1);
                        il.Convert.ToUInt16();
                    });

                done.Branch(@short: true);

                storeZero.Mark();

                StoreVariable(currentInstruction.StoreVariable,
                    valueLoader: () => il.Load(0));

                done.Mark();
            }
        }

        private void op_get_prop_len()
        {
            using (var dataAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<byte>())
            {
                LoadOperand(0);
                dataAddress.Store();

                var done = il.NewLabel();
                var isNotZero = il.NewLabel();

                dataAddress.Load();
                isNotZero.BranchIf(Condition.True, @short: true);

                il.Load(0);
                value.Store();
                done.Branch();

                isNotZero.Mark();

                dataAddress.Load();
                il.Math.Subtract(1);
                il.Convert.ToUInt16();
                dataAddress.Store();

                LoadByte(dataAddress);
                value.Store();

                var checkForZero = il.NewLabel();

                if (machine.Version < 4)
                {
                    value.Load();
                    il.Math.Shr(5);
                    il.Math.Add(1);
                    il.Convert.ToUInt8();
                    value.Store();
                    checkForZero.Branch();
                }

                var secondBranch = il.NewLabel();

                value.Load();
                il.Math.And(0x80);
                secondBranch.BranchIf(Condition.True, @short: true);

                value.Load();
                il.Math.Shr(6);
                il.Math.Add(1);
                il.Convert.ToUInt8();
                value.Store();
                checkForZero.Branch();

                secondBranch.Mark();

                value.Load();
                il.Math.And(0x3f);
                il.Convert.ToUInt8();
                value.Store();

                checkForZero.Mark();

                value.Load();
                done.BranchIf(Condition.True, @short: true);

                il.Load(64);
                value.Store();

                done.Mark();

                StoreVariable(currentInstruction.StoreVariable, value);
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

        private void op_get_sibling()
        {
            using (var result = il.NewLocal<ushort>())
            {
                ReadObjectSiblingFromOperand(0);
                result.Store();

                StoreVariable(currentInstruction.StoreVariable, result);

                result.Load();
                il.Load(0);
                il.Compare.GreaterThan();
                Branch();
            }
        }

        private void op_put_prop()
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var propNum = il.NewLocal<ushort>())
            using (var value = il.NewLocal<byte>())
            using (var propAddress = il.NewLocal<ushort>())
            {
                // Read objNum
                var done = il.NewLabel();
                ReadValidObjectNumber(0, done);
                objNum.Store();

                // Read propNum
                LoadOperand(1);
                propNum.Store();

                il.DebugWrite("propNum: {0}", propNum);

                int mask = machine.Version < 4 ? 0x1f : 0x3f;

                // Read first property address into propAddress
                FirstProperty(objNum);
                propAddress.Store();

                var loopStart = il.NewLabel();
                var loopDone = il.NewLabel();

                il.DebugIndent();
                loopStart.Mark();

                // Read first property byte and store in value
                LoadByte(propAddress);
                value.Store();

                // if ((value & mask) <= propNum) break;
                value.Load();
                il.Math.And(mask);

#if DEBUG
                using (var temp = il.NewLocal<ushort>())
                {
                    temp.Store();
                    il.DebugWrite("property number at address: {0} {1:x4}", temp, propAddress);
                    temp.Load();
                }
#endif

                propNum.Load();
                loopDone.BranchIf(Condition.AtMost, @short: true);

                // Read next property address into propAddress
                propAddress.Load();
                NextProperty();
                propAddress.Store();

                // Branch to start of loop
                loopStart.Branch();

                loopDone.Mark();
                il.DebugUnindent();

                // if ((value & mask) != propNum) throw;
                var propNumFound = il.NewLabel();
                value.Load();
                il.Math.And(mask);
                propNum.Load();
                propNumFound.BranchIf(Condition.Equal, @short: true);
                il.RuntimeError("Object {0} does not contain property {1}", objNum, propNum);

                propNumFound.Mark();

                il.DebugWrite("Found property {0} at address {1:x4}", propNum, propAddress);

                // propAddress++;
                propAddress.Load();
                il.Math.Add(1);
                il.Convert.ToUInt16();
                propAddress.Store();

                var sizeIsWord = il.NewLabel();

                // if ((this.version <= 3 && (value & 0xe0) != 0) && (this.version >= 4) && (value & 0xc0) != 0)
                int sizeMask = machine.Version < 4 ? 0xe0 : 0xc0;
                value.Load();
                il.Math.And(sizeMask);
                sizeIsWord.BranchIf(Condition.True, @short: true);

                // write byte
                using (var temp = il.NewLocal<byte>())
                {
                    LoadOperand(2);
                    il.Convert.ToUInt8();
                    temp.Store();

                    StoreByte(propAddress, temp);

                    il.DebugWrite("Wrote byte {0:x2} to {1:x4}", temp, propAddress);

                    done.Branch(@short: true);
                }

                // write word
                sizeIsWord.Mark();

                using (var temp = il.NewLocal<ushort>())
                {
                    LoadOperand(2);
                    temp.Store();

                    StoreWord(propAddress, temp);

                    il.DebugWrite("Wrote word {0:x2} to {1:x4}", temp, propAddress);
                }

                done.Mark();
            }
        }

        private void op_remove_obj()
        {
            using (var objNum = il.NewLocal<ushort>())
            {
                LoadOperand(0);
                objNum.Store();

                RemoveObjectFromParent(objNum);
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

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Output routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_buffer_mode()
        {
            LoadOperand(0);

            // TODO: What does buffer_mode mean in this terp?
            il.Pop();
        }

        private void op_new_line()
        {
            PrintChar('\n');
        }

        private void op_output_stream()
        {
            var op = GetOperand(0);

            if (op.Kind == OperandKind.Variable)
            {
                throw new ZCompilerException("Expected non-variable operand.");
            }

            switch ((short)op.Value)
            {
                case 1:
                    SelectScreenStream();
                    break;

                case 2:
                    SelectTranscriptStream();
                    break;

                case 3:
                    using (var address = il.NewLocal<ushort>())
                    {
                        LoadOperand(1);
                        address.Store();
                        SelectMemoryStream(address);
                    }
                    break;

                case -1:
                    DeselectScreenStream();
                    break;

                case -2:
                    DeselectTranscriptStream();
                    break;

                case -3:
                    DeselectMemoryStream();
                    break;

                case 4:
                case -4:
                    throw new ZCompilerException("Stream 4 is not supported.");

                default:
                    throw new ZCompilerException(string.Format("Illegal stream value: {0}", op.Value));
            }
        }

        private void op_print()
        {
            var text = machine.ConvertZText(currentInstruction.ZText);
            PrintText(text);
        }

        private void op_print_addr()
        {
            il.LoadArg(0);
            LoadOperand(0);

            var readZText = Reflection<CompiledZMachine>.GetMethod("ReadZText", Types.One<int>(), @public: false);
            il.Call(readZText);
            PrintText();
        }

        private void op_print_char()
        {
            LoadOperand(0);
            PrintChar();
        }

        private void op_print_num()
        {
            using (var number = il.NewLocal<short>())
            {
                LoadOperand(0);
                il.Convert.ToInt16();
                number.Store();

                number.LoadAddress();

                var toString = Reflection<short>.GetMethod("ToString", Types.None);
                il.Call(toString);

                PrintText();
            }
        }

        private void op_print_obj()
        {
            il.LoadArg(0);
            ReadObjectShortNameFromOperand(0);

            var convertZText = Reflection<CompiledZMachine>.GetMethod("ConvertZText", Types.One<ushort[]>(), @public: false);
            il.Call(convertZText);
            PrintText();
        }

        private void op_print_paddr()
        {
            il.LoadArg(0);
            var op = GetOperand(0);
            LoadUnpackedStringAddress(op);

            var readZText = Reflection<CompiledZMachine>.GetMethod("ReadZText", Types.One<int>(), @public: false);
            il.Call(readZText);
            PrintText();
        }

        private void op_print_ret()
        {
            var text = machine.ConvertZText(currentInstruction.ZText);
            PrintText(text);
            Return(1);
        }

        private void op_set_color()
        {
            using (var foreground = il.NewLocal<ZColor>())
            using (var background = il.NewLocal<ZColor>())
            {
                LoadOperand(0);
                foreground.Store();

                LoadOperand(1);
                background.Store();

                SetColor(foreground, background);
            }
        }

        private void op_set_color6()
        {
            NotImplemented();
        }

        private void op_text_style()
        {
            LoadOperand(0);
            SetTextStyle();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Input routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_read_char()
        {
            if (currentInstruction.OperandCount > 0)
            {
                var inputStreamOp = GetOperand(0);
                if (inputStreamOp.Kind == OperandKind.Variable)
                {
                    throw new ZCompilerException("op_read_char: Expected a single non-variable operand");
                }

                if (inputStreamOp.Value != 1)
                {
                    throw new ZCompilerException("op_read_char: Expected a single non-variable operand with value 1, but was " + inputStreamOp.Value);
                }
            }
            else
            {
                throw new ZCompilerException("op_read_char: Expected a single non-variable operand");
            }

            using (var result = il.NewLocal<ushort>())
            {
                il.LoadArg(0);

                var read = Reflection<CompiledZMachine>.GetMethod("ReadChar", Types.None, @public: false);
                il.Call(read);

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_aread()
        {
            il.LoadArg(0);
            LoadOperand(0);

            if (currentInstruction.OperandCount > 1)
            {
                LoadOperand(1);
            }
            else
            {
                il.Load(0);
            }

            if (currentInstruction.OperandCount > 2)
            {
                il.RuntimeError("Timed input not supported");
            }

            using (var result = il.NewLocal<ushort>())
            {
                var read = Reflection<CompiledZMachine>.GetMethod("Read_Z5", Types.Two<ushort, ushort>(), @public: false);
                il.Call(read);

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_sread1()
        {
            il.LoadArg(0);
            LoadOperand(0);
            LoadOperand(1);

            var read = Reflection<CompiledZMachine>.GetMethod("Read_Z3", Types.Two<ushort, ushort>(), @public: false);
            il.Call(read);
        }

        private void op_sread4()
        {
            il.LoadArg(0);
            LoadOperand(0);
            LoadOperand(1);

            if (currentInstruction.OperandCount > 2)
            {
                il.RuntimeError("Timed input not supported");
            }

            var read = Reflection<CompiledZMachine>.GetMethod("Read_Z4", Types.Two<ushort, ushort>(), @public: false);
            il.Call(read);
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Window routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_erase_window()
        {
            LoadOperand(0);
            EraseWindow();
        }

        private void op_set_cursor()
        {
            using (var line = il.NewLocal<ushort>())
            using (var column = il.NewLocal<ushort>())
            {
                LoadOperand(0);
                line.Store();

                LoadOperand(1);
                column.Store();

                SetCursor(line, column);
            }
        }

        private void op_set_cursor6()
        {
            NotImplemented();
        }

        private void op_set_window()
        {
            LoadOperand(0);
            SetWindow();
        }

        private void op_split_window()
        {
            LoadOperand(0);
            SplitWindow();
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        // Miscellaneous routines
        ///////////////////////////////////////////////////////////////////////////////////////////

        private void op_check_arg_count()
        {
            LoadOperand(0);

            il.LoadArg(0);
            var argumentCountField = Reflection<CompiledZMachine>.GetField("argumentCount", @public: false);
            il.Load(argumentCountField);

            il.Compare.AtMost();
            Branch();
        }

        private void op_piracy()
        {
            il.Load(1);
            Branch();
        }

        private void op_verify()
        {
            var verify = Reflection<CompiledZMachine>.GetMethod("Verify", @public: false);

            il.LoadArg(0);
            il.Call(verify);

            Branch();
        }

        private void op_quit()
        {
            Profiler_Quit();
            il.ThrowException<ZMachineQuitException>();
        }

        private void op_random()
        {
            using (var range = il.NewLocal<short>())
            using (var result = il.NewLocal<ushort>())
            {
                var seed = il.NewLabel();
                var done = il.NewLabel();

                LoadOperand(0);
                il.Convert.ToInt16();
                range.Store();

                range.Load();
                il.Load(0);
                seed.BranchIf(Condition.AtMost, @short: true);

                il.LoadArg(0);
                range.Load();

                var nextRandom = Reflection<CompiledZMachine>.GetMethod("NextRandom", Types.One<short>(), @public: false);
                il.Call(nextRandom);
                done.Branch(@short: true);

                seed.Mark();

                il.LoadArg(0);
                range.Load();

                var seedRandom = Reflection<CompiledZMachine>.GetMethod("SeedRandom", Types.One<short>(), @public: false);
                il.Call(seedRandom);
                il.Load(0);

                done.Mark();

                il.Convert.ToUInt16();

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_restart()
        {
            NotImplemented();
        }

        private void op_restore_undo()
        {
            using (var result = il.NewLocal<ushort>())
            {
                il.Load(-1);
                il.Convert.ToUInt16();

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_save_undo()
        {
            using (var result = il.NewLocal<ushort>())
            {
                il.Load(-1);
                il.Convert.ToUInt16();

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_show_status()
        {
            ShowStatus();
        }

        private void op_tokenize()
        {
            il.LoadArg(0);

            LoadOperand(0);
            LoadOperand(1);

            if (currentInstruction.OperandCount > 2)
            {
                LoadOperand(2);
            }
            else
            {
                il.Load(0);
            }

            if (currentInstruction.OperandCount > 3)
            {
                LoadOperand(3);

                il.Load(0);
                il.Compare.Equal();
                il.Load(0);
                il.Compare.Equal();
            }
            else
            {
                il.Load(0);
            }

            var tokenize = Reflection<CompiledZMachine>.GetMethod("Tokenize", Types.Four<ushort, ushort, ushort, bool>(), @public: false);
            il.Call(tokenize);
        }
    }
}
