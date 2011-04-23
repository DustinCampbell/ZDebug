using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PutPropGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand propertyOp;
        private readonly Operand valueOp;

        public PutPropGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.propertyOp = instruction.Operands[1];
            this.valueOp = instruction.Operands[2];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            {
                // Read objNum
                var done = il.NewLabel();
                compiler.EmitLoadValidObject(objectOp, done);
                objNum.Store();

                using (var propNum = il.NewLocal<ushort>())
                using (var value = il.NewLocal<byte>())
                using (var propAddress = il.NewLocal<ushort>())
                {
                    // Read propNum
                    compiler.EmitLoadOperand(propertyOp);
                    propNum.Store();

                    il.DebugWrite("propNum: {0}", propNum);

                    int mask = compiler.Version < 4 ? 0x1f : 0x3f;

                    // Read first property address into propAddress
                    compiler.EmitLoadFirstPropertyAddress(objNum);
                    propAddress.Store();

                    var loopStart = il.NewLabel();
                    var loopDone = il.NewLabel();

                    il.DebugIndent();
                    loopStart.Mark();

                    // Read first property byte and store in value
                    compiler.EmitLoadMemoryByte(propAddress);
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
                    compiler.EmitLoadNextPropertyAddress();
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
                    int sizeMask = compiler.Version < 4 ? 0xe0 : 0xc0;
                    value.Load();
                    il.Math.And(sizeMask);
                    sizeIsWord.BranchIf(Condition.True, @short: true);

                    // write byte
                    using (var temp = il.NewLocal<byte>())
                    {
                        compiler.EmitLoadOperand(valueOp);
                        il.Convert.ToUInt8();
                        temp.Store();

                        compiler.EmitStoreMemoryByte(propAddress, temp);

                        il.DebugWrite("Wrote byte {0:x2} to {1:x4}", temp, propAddress);

                        done.Branch(@short: true);
                    }

                    // write word
                    sizeIsWord.Mark();

                    using (var temp = il.NewLocal<ushort>())
                    {
                        compiler.EmitLoadOperand(valueOp);
                        temp.Store();

                        compiler.EmitStoreMemoryWord(propAddress, temp);

                        il.DebugWrite("Wrote word {0:x2} to {1:x4}", temp, propAddress);
                    }
                }

                done.Mark();
            }
        }
    }
}
