using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;
using System.Reflection.Emit;
using System.Reflection;
using ZDebug.Compiler.Generate;
using ZDebug.Core.Execution;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private readonly static MethodInfo shortToString = typeof(short).GetMethod(
            name: "ToString",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: new Type[0],
            modifiers: null);

        private void call()
        {
            using (var address = il.NewLocal<int>())
            using (var args = il.NewArrayLocal<ushort>(currentInstruction.OperandCount - 1))
            {
                UnpackRoutineAddress(GetOperand(0));
                address.Store();

                for (int j = 1; j < currentInstruction.OperandCount; j++)
                {
                    // don't close over the iterator variable
                    int index = j;

                    args.StoreElement(
                        il.GenerateLoad(index - 1),
                        il.Generate(() =>
                            ReadOperand(index)));
                }

                var legalCall = il.NewLabel();
                address.Load();
                legalCall.BranchIf(Condition.True, @short: true);

                var done = il.NewLabel();
                il.Load(0);

                done.Branch(@short: true);

                legalCall.Mark();
                il.LoadArg(0);
                address.Load();
                args.Load();

                il.Call(callHelper);

                done.Mark();
            }

        }

        private void op_call_n()
        {
            il.DebugIndent();

            call();

            // discard result...
            il.Pop();

            il.DebugUnindent();
        }

        private void op_call_s()
        {
            il.DebugIndent();

            using (var result = il.NewLocal<ushort>())
            {
                call();

                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }

            il.DebugUnindent();
        }

        private void op_check_arg_count()
        {
            ReadOperand(0);
            argCount.Load();

            il.Compare.AtMost();
            Branch();
        }

        private void op_not()
        {
            ReadOperand(0);
            il.Math.Not();
            il.Convert.ToUInt16();

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_print_char()
        {
            ReadOperand(0);
            PrintChar();
        }

        private void op_print_num()
        {
            using (var number = il.NewLocal<short>())
            {
                ReadOperand(0);
                il.Convert.ToInt16();
                number.Store();

                number.LoadAddress();
                il.Call(shortToString);

                PrintText();
            }
        }

        private void op_pull()
        {
            using (var varIndex = il.NewLocal<byte>())
            using (var value = il.NewLocal<ushort>())
            {
                ReadByRefVariableOperand();
                varIndex.Store();

                PopStack();

                value.Store();
                WriteVariable(varIndex, value, indirect: true);
            }

        }

        private void op_push()
        {
            ReadOperand(0);
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
                ReadOperand(1);
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
                ReadByte(propAddress);
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
                    ReadOperand(2);
                    il.Convert.ToUInt8();
                    temp.Store();

                    WriteByte(propAddress, temp);

                    il.DebugWrite("Wrote byte {0:x2} to {1:x4}", temp, propAddress);

                    done.Branch(@short: true);
                }

                // write word
                sizeIsWord.Mark();

                using (var temp = il.NewLocal<ushort>())
                {
                    ReadOperand(2);
                    temp.Store();

                    WriteWord(propAddress, temp);

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

                ReadOperand(0);
                il.Convert.ToInt16();
                range.Store();

                range.Load();
                il.Load(0);
                seed.BranchIf(Condition.AtMost, @short: true);

                il.LoadArg(0);
                range.Load();
                il.Call(nextRandomHelper);
                done.Branch(@short: true);

                seed.Mark();

                il.LoadArg(0);
                range.Load();
                il.Call(seedRandomHelper);
                il.Load(0);

                done.Mark();

                il.Convert.ToUInt16();

                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_text_style()
        {
            ReadOperand(0);
            SetTextStyle();
        }

        private void op_buffer_mode()
        {
            ReadOperand(0);

            // TODO: What does buffer_mode mean in this terp?
            il.Pop();
        }

        private void op_erase_window()
        {
            ReadOperand(0);
            EraseWindow();
        }

        private void op_split_window()
        {
            ReadOperand(0);
            SplitWindow();
        }

        private void op_set_window()
        {
            ReadOperand(0);
            SetWindow();
        }

        private void op_set_color()
        {
            using (var foreground = il.NewLocal<ZColor>())
            using (var background = il.NewLocal<ZColor>())
            {
                ReadOperand(0);
                foreground.Store();

                ReadOperand(1);
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
                ReadOperand(0);
                line.Store();

                ReadOperand(1);
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
                ReadOperand(0);
                ReadOperand(1);
                il.Math.Add();
                address.Store();

                ReadOperand(2);
                il.Convert.ToUInt8();
                value.Store();

                WriteByte(address, value);
            }
        }

        private void op_storew()
        {
            using (var address = il.NewLocal<int>())
            using (var value = il.NewLocal<ushort>())
            {
                ReadOperand(0);
                ReadOperand(1);
                il.Math.Multiply(2);
                il.Math.Add();
                address.Store();

                ReadOperand(2);
                value.Store();

                WriteWord(address, value);
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
                        ReadOperand(1);
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
                il.Call(readCharHelper);

                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_sread1()
        {
            il.LoadArg(0);
            ReadOperand(0);
            ReadOperand(1);

            il.Call(readZ3Helper);
        }

        private void op_sread4()
        {
            il.LoadArg(0);
            ReadOperand(0);
            ReadOperand(1);

            if (currentInstruction.OperandCount > 2)
            {
                il.RuntimeError("Timed input not supported");
            }

            il.Call(readZ4Helper);
        }

        private void op_aread()
        {
            il.LoadArg(0);
            ReadOperand(0);

            if (currentInstruction.OperandCount > 1)
            {
                ReadOperand(1);
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
                il.Call(readZ5Helper);

                result.Store();
                WriteVariable(currentInstruction.StoreVariable, result);
            }
        }

        private void op_tokenize()
        {
            il.LoadArg(0);

            ReadOperand(0);
            ReadOperand(1);

            if (currentInstruction.OperandCount > 2)
            {
                ReadOperand(2);
            }
            else
            {
                il.Load(0);
            }

            if (currentInstruction.OperandCount > 3)
            {
                ReadOperand(3);

                il.Load(0);
                il.Compare.Equal();
                il.Load(0);
                il.Compare.Equal();
            }
            else
            {
                il.Load(0);
            }

            il.Call(tokenizeHelper);
        }
    }
}
