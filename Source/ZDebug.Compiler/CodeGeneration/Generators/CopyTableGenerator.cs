using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class CopyTableGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Operand op3;

        public CopyTableGenerator(Operand op1, Operand op2, Operand op3)
            : base(OpcodeGeneratorKind.CopyTable)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.op3 = op3;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.LoadThis();
            compiler.EmitLoadOperand(op1);
            compiler.EmitLoadOperand(op2);
            compiler.EmitLoadOperand(op3);
            il.Call(Reflection<CompiledZMachine>.GetMethod("op_copy_table", Types.Three<ushort, ushort, ushort>(), @public: false));
        }
    }
}
