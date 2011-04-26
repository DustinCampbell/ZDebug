using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class InsertObjGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand destinationOp;

        public InsertObjGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.destinationOp = instruction.Operands[1];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var destNum = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                compiler.EmitLoadValidObject(objectOp, done, reuse: ReuseFirstOperand);
                objNum.Store();

                compiler.EmitLoadValidObject(destinationOp, done);
                destNum.Store();

                compiler.EmitObjectMoveToDestination(objNum, destNum);

                done.Mark();
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
