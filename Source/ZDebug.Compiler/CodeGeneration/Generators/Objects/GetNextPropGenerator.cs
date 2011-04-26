using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class GetNextPropGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand propertyOp;
        private readonly Variable store;

        public GetNextPropGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.propertyOp = instruction.Operands[1];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var propNum = il.NewLocal<ushort>())
            using (var propAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                // Read objNum
                var invalidObjNum = il.NewLabel();
                compiler.EmitLoadValidObject(objectOp, invalidObjNum, reuse: ReuseFirstOperand);
                objNum.Store();

                // Read propNum
                compiler.EmitLoadOperand(propertyOp);
                propNum.Store();

                int mask = compiler.Version < 4 ? 0x1f : 0x3f;

                // Read first property address into propAddress
                compiler.EmitLoadFirstPropertyAddress(objNum);
                propAddress.Store();

                var storePropNum = il.NewLabel();

                // propNum == 0?
                propNum.Load();
                storePropNum.BranchIf(Condition.False);

                var loopStart = il.NewLabel();

                // start of the loop
                loopStart.Mark();

                compiler.EmitLoadMemoryByte(propAddress);
                value.Store();

                propAddress.Load();
                compiler.EmitLoadNextPropertyAddress();
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

                compiler.EmitLoadMemoryByte(propAddress);
                il.Load(mask);
                il.Math.And();
                il.Convert.ToUInt16();
                value.Store();
                compiler.EmitStoreVariable(store, value, reuse: ReuseStoreVariable);

                done.Branch(@short: true);

                // invalid object encountered
                invalidObjNum.Mark();

                il.Load(0);
                value.Store();
                compiler.EmitStoreVariable(store, value, reuse: ReuseStoreVariable);

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
