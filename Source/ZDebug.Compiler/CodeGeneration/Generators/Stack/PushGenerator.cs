using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PushGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public PushGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadOperand(op);

            using (var value = il.NewLocal<ushort>())
            {
                value.Store();
                compiler.EmitPushStack(value);
            }
        }
    }
}
