using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SetAttrGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand attributeOp;

        public SetAttrGenerator(Instruction instruction)
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
                compiler.EmitLoadValidObject(objectOp, invalidObjNum);
                objNum.Store();

                // Read attribute
                compiler.EmitLoadOperand(attributeOp);
                attribute.Store();

                compiler.EmitObjectChangeAttribute(objNum, attribute, value: true);

                invalidObjNum.Mark();
            }
        }
    }
}
