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

        public ScanTableGenerator(Instruction instruction)
            : base(instruction)
        {
            this.op1 = instruction.Operands[0];
            this.op2 = instruction.Operands[1];
            this.op3 = instruction.Operands[2];
            this.op4 = (instruction.OperandCount > 3 ? instruction.Operands[3] : (Operand?)null);
            this.store = instruction.StoreVariable;
            this.branch = instruction.Branch;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            il.Arguments.LoadMachine();
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

            il.Call(Reflection<CompiledZMachine>.GetMethod("op_scan_table", Types.Array<ushort, ushort, ushort, ushort>(), @public: false));

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
