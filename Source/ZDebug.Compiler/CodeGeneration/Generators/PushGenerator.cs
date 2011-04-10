using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PushGenerator : OpcodeGenerator
    {
        private readonly Operand op;

        public PushGenerator(Operand op)
            : base(OpcodeGeneratorKind.Push)
        {
            this.op = op;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            compiler.EmitLoadOperand(op);

            using (var value = il.NewLocal<ushort>())
            {
                compiler.EmitPushStack(value);
            }
        }
    }
}
