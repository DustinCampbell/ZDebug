using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class SetCursorGenerator : OpcodeGenerator
    {
        private readonly Operand lineOp;
        private readonly Operand columnOp;

        public SetCursorGenerator(Instruction instruction)
            : base(instruction)
        {
            this.lineOp = instruction.Operands[0];
            this.columnOp = instruction.Operands[1];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var line = il.NewLocal<ushort>())
            using (var column = il.NewLocal<ushort>())
            {
                compiler.EmitLoadOperand(lineOp);
                line.Store();

                compiler.EmitLoadOperand(columnOp);
                column.Store();

                compiler.EmitSetCursor(line, column);
            }

        }
    }
}
