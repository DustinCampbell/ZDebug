using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class VerifyGenerator : OpcodeGenerator
    {
        private readonly Branch branch;

        public VerifyGenerator(Instruction instruction)
            : base(instruction)
        {
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
            il.Call(Reflection<CompiledZMachine>.GetMethod("Verify", @public: false));

            compiler.EmitBranch(branch);
        }
    }
}
