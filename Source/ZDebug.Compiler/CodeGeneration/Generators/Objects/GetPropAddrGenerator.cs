using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class GetPropAddrGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand propertyOp;
        private readonly Variable store;

        public GetPropAddrGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.propertyOp = instruction.Operands[1];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
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

                    var storeAddress = il.NewLabel();

                    value.Load();
                    il.Math.And(mask);
                    propNum.Load();
                    invalidObjNum.BranchIf(Condition.NotEqual, @short: true);

                    if (compiler.Version > 3)
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

                    using (var result = il.NewLocal<ushort>())
                    {
                        propAddress.Load();
                        il.Math.Add(1);
                        il.Convert.ToUInt16();

                        result.Store();

                        compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
                    }

                    done.Branch(@short: true);
                }

                invalidObjNum.Mark();

                using (var result = il.NewLocal<ushort>())
                {
                    il.Load(0);
                    result.Store();

                    compiler.EmitStoreVariable(store, result, reuse: ReuseStoreVariable);
                }

                done.Mark();
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
