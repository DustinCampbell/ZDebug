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
            if (ReuseFirstOperand)
            {
                using (var shortName = il.NewArrayLocal<ushort>())
                {
                    compiler.EmitLoadObjectShortName(objectOp, reuse: true);
                    shortName.Store();

                    il.Arguments.LoadMachine();
                    shortName.Load();
                }
            }
            else
            {
                il.Arguments.LoadMachine();
                compiler.EmitLoadObjectShortName(objectOp);
            }

            il.Call(Reflection<CompiledZMachine>.GetMethod("ConvertZText", Types.Array<ushort[]>(), @public: false));

            compiler.EmitPrintText();
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
