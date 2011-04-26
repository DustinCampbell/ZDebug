using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class RemoveObjGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;

        public RemoveObjGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            {
                if (!ReuseFirstOperand)
                {
                    compiler.EmitLoadOperand(objectOp);
                }

                objNum.Store();

                compiler.EmitObjectRemoveFromParent(objNum);
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
