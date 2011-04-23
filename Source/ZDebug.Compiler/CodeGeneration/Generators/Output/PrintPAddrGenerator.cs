using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintPAddrGenerator : OpcodeGenerator
    {
        private readonly Operand address;

        public PrintPAddrGenerator(Instruction instruction)
            : base(instruction)
        {
            this.address = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
            compiler.EmitLoadUnpackedStringAddress(address);

            il.Call(Reflection<CompiledZMachine>.GetMethod("ReadZText", Types.Array<int>(), @public: false));

            compiler.EmitPrintText();
        }
    }
}
