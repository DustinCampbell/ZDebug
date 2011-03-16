using ZDebug.Compiler.Generate;
using ZDebug.Compiler.Utilities;
using ZDebug.Core.Execution;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
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

        private void op_check_arg_count()
        {
            LoadOperand(0);

            il.LoadArg(0);
            var argumentCountField = Reflection<ZMachine>.GetField("argumentCount", @public: false);
            il.Load(argumentCountField);

            il.Compare.AtMost();
            Branch();
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

        private void op_pull()
        {
            using (var varIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<ushort>())
            {
                LoadByRefVariableOperand();
                varIndex.Store();

                PopStack();

                value.Store();
                StoreVariable(varIndex, value, indirect: true);
            }

        }

        private void op_push()
        {
            LoadOperand(0);
            PushStack();
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

                var nextRandom = Reflection<ZMachine>.GetMethod("NextRandom", Types.One<short>(), @public: false);
                il.Call(nextRandom);
                done.Branch(@short: true);

                seed.Mark();

                il.LoadArg(0);
                range.Load();

                var seedRandom = Reflection<ZMachine>.GetMethod("SeedRandom", Types.One<short>(), @public: false);
                il.Call(seedRandom);
                il.Load(0);

                done.Mark();

                il.Convert.ToUInt16();

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_text_style()
        {
            LoadOperand(0);
            SetTextStyle();
        }

        private void op_buffer_mode()
        {
            LoadOperand(0);

            // TODO: What does buffer_mode mean in this terp?
            il.Pop();
        }

        private void op_erase_window()
        {
            LoadOperand(0);
            EraseWindow();
        }

        private void op_split_window()
        {
            LoadOperand(0);
            SplitWindow();
        }

        private void op_set_window()
        {
            LoadOperand(0);
            SetWindow();
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

        private void op_storeb()
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

        private void op_storew()
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

                var read = Reflection<ZMachine>.GetMethod("ReadChar", Types.None, @public: false);
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

            var read = Reflection<ZMachine>.GetMethod("Read_Z3", Types.Two<ushort, ushort>(), @public: false);
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

            var read = Reflection<ZMachine>.GetMethod("Read_Z4", Types.Two<ushort, ushort>(), @public: false);
            il.Call(read);
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
                var read = Reflection<ZMachine>.GetMethod("Read_Z5", Types.Two<ushort, ushort>(), @public: false);
                il.Call(read);

                result.Store();
                StoreVariable(currentInstruction.StoreVariable, result);
            }
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

            var tokenize = Reflection<ZMachine>.GetMethod("Tokenize", Types.Four<ushort, ushort, ushort, bool>(), @public: false);
            il.Call(tokenize);
        }
    }
}
