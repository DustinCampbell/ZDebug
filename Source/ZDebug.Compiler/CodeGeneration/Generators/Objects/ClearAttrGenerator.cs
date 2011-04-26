using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class ClearAttrGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand attributeOp;

        public ClearAttrGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.attributeOp = instruction.Operands[1];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var attribute = il.NewLocal<byte>())
            {
                // Read objNum
                var invalidObjNum = il.NewLabel();
                compiler.EmitLoadValidObject(objectOp, invalidObjNum, reuse: ReuseFirstOperand);
                objNum.Store();

                // Read attribute
                compiler.EmitLoadOperand(attributeOp);
                attribute.Store();

                compiler.EmitObjectChangeAttribute(objNum, attribute, value: false);

                invalidObjNum.Mark();
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
