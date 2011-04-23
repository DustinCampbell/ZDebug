using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class TestAttrGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand attributeOp;
        private readonly Branch branch;

        public TestAttrGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.attributeOp = instruction.Operands[1];
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var attribute = il.NewLocal<byte>())
            {
                // Read objNum
                var invalidObjNum = il.NewLabel();
                compiler.EmitLoadValidObject(objectOp, invalidObjNum);
                objNum.Store();

                // Read attribute
                compiler.EmitLoadOperand(attributeOp);
                attribute.Store();

                compiler.EmitObjectHasAttribute(objNum, attribute);
                compiler.EmitBranch(branch);

                var done = il.NewLabel();
                done.Branch(@short: true);

                invalidObjNum.Mark();
                il.Load(false);
                compiler.EmitBranch(branch);

                done.Mark();
            }
        }
    }
}
