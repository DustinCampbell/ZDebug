using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class GetPropGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand propertyOp;
        private readonly Variable store;

        public GetPropGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.propertyOp = instruction.Operands[1];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var result = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                // Read objNum
                var invalidObjNum = il.NewLabel();
                compiler.EmitLoadValidObject(objectOp, invalidObjNum, reuse: ReuseFirstOperand);
                objNum.Store();

                using (var propNum = il.NewLocal<ushort>())
                using (var propAddress = il.NewLocal<ushort>())
                using (var value = il.NewLocal<ushort>())
                {
                    // Read propNum
                    compiler.EmitLoadOperand(propertyOp);
                    propNum.Store();

                    int mask = compiler.Version < 4 ? 0x1f : 0x3f;

                    // Read first property address into propAddress
                    compiler.EmitLoadFirstPropertyAddress(objNum);
                    propAddress.Store();

                    var loopStart = il.NewLabel();
                    var loopDone = il.NewLabel();

                    loopStart.Mark();

                    compiler.EmitLoadMemoryByte(propAddress);
                    value.Store();

                    value.Load();
                    il.Math.And(mask);
                    il.Convert.ToUInt16();
                    propNum.Load();
                    loopDone.BranchIf(Condition.AtMost, @short: true);

                    propAddress.Load();
                    compiler.EmitLoadNextPropertyAddress();
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

                    var sizeMask = compiler.Version < 4 ? 0xe0 : 0xc0;

                    var secondBranch = il.NewLabel();

                    value.Load();
                    il.Math.And(sizeMask);
                    secondBranch.BranchIf(Condition.True, @short: true);

                    compiler.EmitLoadMemoryByte(propAddress);
                    result.Store();

                    done.Branch();

                    secondBranch.Mark();

                    compiler.EmitLoadMemoryWord(propAddress);
                    result.Store();

                    done.Branch();

                    propNotFound.Mark();

                    compiler.EmitLoadDefaultPropertyAddress(propNum);
                    propAddress.Store();

                    compiler.EmitLoadMemoryWord(propAddress);
                    result.Store();

                    done.Branch();
                }

                invalidObjNum.Mark();

                il.Load(0);
                result.Store();

                done.Mark();

                compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }

        public override bool CanReuseStoreVariable
        {
            get { return true; }
        }
    }
}
