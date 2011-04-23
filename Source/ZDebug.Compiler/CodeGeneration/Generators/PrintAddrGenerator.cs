using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintAddrGenerator : OpcodeGenerator
    {
        private readonly Operand addressOp;

        public PrintAddrGenerator(Instruction instruction)
            : base(instruction)
        {
            this.addressOp = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
            compiler.EmitLoadOperand(addressOp);

            il.Call(Reflection<CompiledZMachine>.GetMethod("ReadZText", Types.Array<int>(), @public: false));

            compiler.EmitPrintText();
        }
    }
}
