using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class PrintObjGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;

        public PrintObjGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
            compiler.EmitLoadObjectShortName(objectOp);

            il.Call(Reflection<CompiledZMachine>.GetMethod("ConvertZText", Types.Array<ushort[]>(), @public: false));

            compiler.EmitPrintText();
        }
    }
}
