using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class ScanTableGenerator : OpcodeGenerator
    {
        private readonly Operand op1;
        private readonly Operand op2;
        private readonly Operand op3;
        private readonly Operand? op4;
        private readonly Variable store;
        private readonly Branch branch;

        public ScanTableGenerator(Operand op1, Operand op2, Operand op3, Operand? op4, Variable store, Branch branch)
            : base(OpcodeGeneratorKind.ScanTable)
        {
            this.op1 = op1;
            this.op2 = op2;
            this.op3 = op3;
            this.op4 = op4;
            this.store = store;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.LoadThis();
            compiler.EmitLoadOperand(op1);
            compiler.EmitLoadOperand(op2);
            compiler.EmitLoadOperand(op3);

            if (op4.HasValue)
            {
                compiler.EmitLoadOperand(op4.Value);
            }
            else
            {
                il.Load(0x82);
            }

            il.Call(Reflection<CompiledZMachine>.GetMethod("op_scan_table", Types.Four<ushort, ushort, ushort, ushort>(), @public: false));

            using (var result = il.NewLocal<ushort>())
            {
                result.Store();
                compiler.EmitStoreVariable(store, result);

                result.Load();
                il.Load(0);
                il.Compare.NotEqual();
            }

            compiler.EmitBranch(branch);
        }
    }
}
