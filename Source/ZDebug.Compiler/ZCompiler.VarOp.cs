using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZDebug.Core.Instructions;
using System.Reflection.Emit;
using System.Reflection;

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
            using (var address = localManager.AllocateTemp<int>())
            using (var args = localManager.AllocateTemp<ushort[]>())
            {
                UnpackRoutineAddress(currentInstruction.Operands[0]);
                il.Emit(OpCodes.Stloc, address);

                il.DebugWrite("calling {0:x4}...", address);

                il.Emit(OpCodes.Ldc_I4, currentInstruction.OperandCount - 1);
                il.Emit(OpCodes.Newarr, typeof(ushort));
                il.Emit(OpCodes.Stloc, args);

                for (int j = 1; j < currentInstruction.OperandCount; j++)
                {
                    il.Emit(OpCodes.Ldloc, args);
                    il.Emit(OpCodes.Ldc_I4, j - 1);
                    ReadOperand(currentInstruction.Operands[j]);
                    il.Emit(OpCodes.Stelem_I2);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc, address);
                il.Emit(OpCodes.Ldloc, args);

                il.Emit(OpCodes.Call, callHelper);
            }

        }

        private void op_call_n()
        {
            il.DebugIndent();

            call();

            // discard result...
            il.Emit(OpCodes.Pop);

            il.DebugUnindent();
        }

        private void op_call_s()
        {
            il.DebugIndent();

            using (var result = localManager.AllocateTemp<ushort>())
            {
                call();

                il.Emit(OpCodes.Stloc, result);
                WriteVariable(currentInstruction.StoreVariable, result);
            }

            il.DebugUnindent();
        }

        private void op_print_char()
        {
            ReadOperand(currentInstruction.Operands[0]);
            PrintChar();
        }

        private void op_print_num()
        {
            using (var number = localManager.AllocateTemp<short>())
            {
                ReadOperand(currentInstruction.Operands[0]);
                il.Emit(OpCodes.Conv_I2);
                il.Emit(OpCodes.Stloc, number);

                il.Emit(OpCodes.Ldloca_S, number);
                il.Emit(OpCodes.Call, shortToString);

                PrintText();
            }
        }

        private void op_put_prop()
        {
            using (var objNum = localManager.AllocateTemp<ushort>())
            using (var propNum = localManager.AllocateTemp<ushort>())
            using (var value = localManager.AllocateTemp<byte>())
            using (var propAddress = localManager.AllocateTemp<ushort>())
            {
                // Read objNum
                var done = il.DefineLabel();
                ReadValidObjectNumber(currentInstruction.Operands[0], done);
                il.Emit(OpCodes.Stloc, objNum);

                // Read propNum
                ReadOperand(currentInstruction.Operands[1]);
                il.Emit(OpCodes.Stloc, propNum);

                il.DebugWrite("propNum: {0}", propNum);

                int mask = machine.Version < 4 ? 0x1f : 0x3f;

                // Read first property address into propAddress
                FirstProperty(objNum);
                il.Emit(OpCodes.Stloc, propAddress);

                var loopStart = il.DefineLabel();
                var loopDone = il.DefineLabel();

                il.DebugIndent();
                il.MarkLabel(loopStart);

                // Read first property byte and store in value
                ReadByte(propAddress);
                il.Emit(OpCodes.Stloc, value);

                // if ((value & mask) <= propNum) break;
                il.Emit(OpCodes.Ldloc, value);
                il.Emit(OpCodes.Ldc_I4, mask);
                il.Emit(OpCodes.And);

#if DEBUG
                using (var temp = localManager.AllocateTemp<ushort>())
                {
                    il.Emit(OpCodes.Stloc, temp);
                    il.DebugWrite("property number at address: {0} {1:x4}", temp, propAddress);
                    il.Emit(OpCodes.Ldloc, temp);
                }
#endif

                il.Emit(OpCodes.Ldloc, propNum);
                il.Emit(OpCodes.Ble_S, loopDone);

                // Read next property address into propAddress
                il.Emit(OpCodes.Ldloc, propAddress);
                NextProperty();
                il.Emit(OpCodes.Stloc, propAddress);

                // Branch to start of loop
                il.Emit(OpCodes.Br, loopStart);

                il.MarkLabel(loopDone);
                il.DebugUnindent();

                // if ((value & mask) != propNum) throw;
                var propNumFound = il.DefineLabel();
                il.Emit(OpCodes.Ldloc, value);
                il.Emit(OpCodes.Ldc_I4, mask);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Ldloc, propNum);
                il.Emit(OpCodes.Beq_S, propNumFound);
                il.ThrowException("Object {0} does not contain property {1}", objNum, propNum);

                il.MarkLabel(propNumFound);

                il.DebugWrite("Found property {0} at address {1:x4}", propNum, propAddress);

                // propAddress++;
                il.Emit(OpCodes.Ldloc, propAddress);
                il.Emit(OpCodes.Ldc_I4_1);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Conv_U2);
                il.Emit(OpCodes.Stloc, propAddress);

                var sizeIsWord = il.DefineLabel();

                // if ((this.version <= 3 && (value & 0xe0) != 0) && (this.version >= 4) && (value & 0xc0) != 0)
                int sizeMask = machine.Version < 4 ? 0xe0 : 0xc0;
                il.Emit(OpCodes.Ldloc, value);
                il.Emit(OpCodes.Ldc_I4, sizeMask);
                il.Emit(OpCodes.And);
                il.Emit(OpCodes.Brtrue_S, sizeIsWord);

                // write byte
                using (var temp = localManager.AllocateTemp<byte>())
                {
                    ReadOperand(currentInstruction.Operands[2]);
                    il.Emit(OpCodes.Conv_U1);
                    il.Emit(OpCodes.Stloc, temp);

                    WriteByte(propAddress, temp);

                    il.DebugWrite("Wrote byte {0:x2} to {1:x4}", temp, propAddress);

                    il.Emit(OpCodes.Br_S, done);
                }

                // write word
                il.MarkLabel(sizeIsWord);

                using (var temp = localManager.AllocateTemp<ushort>())
                {
                    ReadOperand(currentInstruction.Operands[2]);
                    il.Emit(OpCodes.Stloc, temp);

                    WriteWord(propAddress, temp);

                    il.DebugWrite("Wrote word {0:x2} to {1:x4}", temp, propAddress);
                }

                il.MarkLabel(done);
            }
        }

        private void op_storew()
        {
            using (var address = localManager.AllocateTemp<int>())
            using (var value = localManager.AllocateTemp<ushort>())
            {
                ReadOperand(currentInstruction.Operands[0]);
                ReadOperand(currentInstruction.Operands[1]);
                il.Emit(OpCodes.Ldc_I4_2);
                il.Emit(OpCodes.Mul);
                il.Emit(OpCodes.Add);
                il.Emit(OpCodes.Stloc, address);

                ReadOperand(currentInstruction.Operands[2]);
                il.Emit(OpCodes.Stloc, value);

                WriteWord(address, value);
            }
        }
    }
}
