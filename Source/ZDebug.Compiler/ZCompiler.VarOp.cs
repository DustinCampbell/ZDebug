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
        private void op_call_s(Instruction i)
        {
            // TODO: Can we do better here? Allocating a new array to hold
            // every call's arguments might be expensive. We should share
            // this at some point in the future.

            using (var address = localManager.AllocateTemp<int>())
            using (var args = localManager.AllocateTemp<ushort[]>())
            using (var result = localManager.AllocateTemp<ushort>())
            {
                UnpackRoutineAddress(i.Operands[0]);
                il.Emit(OpCodes.Stloc, address);

                il.Emit(OpCodes.Ldc_I4, i.OperandCount - 1);
                il.Emit(OpCodes.Newarr, typeof(ushort));
                il.Emit(OpCodes.Stloc, args);

                for (int j = 1; j < i.OperandCount; j++)
                {
                    il.Emit(OpCodes.Ldloc, args);
                    il.Emit(OpCodes.Ldc_I4, j - 1);
                    ReadOperand(i.Operands[j]);
                    il.Emit(OpCodes.Stelem_I2);
                }

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldloc, address);
                il.Emit(OpCodes.Ldloc, args);

                il.Emit(OpCodes.Call, callHelper);

                il.Emit(OpCodes.Stloc, result);
                WriteVariable(i.StoreVariable, result);
            }
        }

        private void op_put_prop(Instruction i)
        {
            il.DebugIndent();

            using (var objNum = localManager.AllocateTemp<ushort>())
            using (var propNum = localManager.AllocateTemp<ushort>())
            using (var value = localManager.AllocateTemp<byte>())
            using (var propAddress = localManager.AllocateTemp<ushort>())
            {
                // Read object number
                ReadOperand(i.Operands[0]);
                il.Emit(OpCodes.Stloc, objNum);

                il.DebugWrite("objNum: {0}", objNum);

                // Check to see if object number is 0.
                il.Emit(OpCodes.Ldloc, objNum);
                var objNumOk = il.DefineLabel();
                il.Emit(OpCodes.Brtrue_S, objNumOk);

                // TODO: Emit warning messsage

                // Jump to end
                var done = il.DefineLabel();
                il.Emit(OpCodes.Br, done);

                il.MarkLabel(objNumOk);

                il.DebugWrite("objNum is OK (not 0)");

                // Read property number
                ReadOperand(i.Operands[1]);
                il.Emit(OpCodes.Stloc, propNum);

                il.DebugWrite("propNum: {0}", propNum);

                int mask = machine.Version < 4 ? 0x1f : 0x3f;

                // Read first property address into propAddress
                FirstProperty(objNum);
                il.Emit(OpCodes.Stloc, propAddress);

                il.DebugWrite("first property address: {0:x4}", propAddress);

                var loopStart = il.DefineLabel();
                var loopDone = il.DefineLabel();

                il.DebugIndent();
                il.MarkLabel(loopStart);

                // Read first property byte and store in value
                ReadByte(propAddress);
                il.Emit(OpCodes.Stloc, value);

                il.DebugWrite("property byte: {0:x2}", value);

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

                il.DebugWrite("next property address: {0:x4}", propAddress);

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
                    ReadOperand(i.Operands[2]);
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
                    ReadOperand(i.Operands[2]);
                    il.Emit(OpCodes.Stloc, temp);

                    WriteWord(propAddress, temp);

                    il.DebugWrite("Wrote word {0:x2} to {1:x4}", temp, propAddress);
                }

                il.MarkLabel(done);
            }

            il.DebugUnindent();
        }

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
