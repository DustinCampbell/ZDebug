using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class InsertObjGenerator : OpcodeGenerator
    {
        private readonly Operand objectOp;
        private readonly Operand destinationOp;

        public InsertObjGenerator(Instruction instruction)
            : base(instruction)
        {
            this.objectOp = instruction.Operands[0];
            this.destinationOp = instruction.Operands[1];
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var objNum = il.NewLocal<ushort>())
            using (var destNum = il.NewLocal<ushort>())
            {
                var done = il.NewLabel();

                compiler.EmitLoadValidObject(objectOp, done, reuse: ReuseFirstOperand);
                objNum.Store();

                compiler.EmitLoadValidObject(destinationOp, done);
                destNum.Store();

                il.Arguments.LoadMachine();
                objNum.Load();
                destNum.Load();

                il.Call(Reflection<CompiledZMachine>.GetMethod("op_insert_obj", Types.Array<ushort, ushort>(), @public: false));

                done.Mark();
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }
    }
}
