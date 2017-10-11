using ZDebug.Compiler.Generate;
using ZDebug.Core.Instructions;

namespace ZDebug.Compiler.CodeGeneration.Generators
{
    internal class GetPropLenGenerator : OpcodeGenerator
    {
        private readonly Operand dataAddressOp;
        private readonly Variable store;

        public GetPropLenGenerator(Instruction instruction)
            : base(instruction)
        {
            this.dataAddressOp = instruction.Operands[0];
            this.store = instruction.StoreVariable;
        }

        public override void Generate(ILBuilder il, ICompiler compiler)
        {
            using (var dataAddress = il.NewLocal<ushort>())
            using (var value = il.NewLocal<byte>())
            {
                if (!ReuseFirstOperand)
                {
                    compiler.EmitLoadOperand(dataAddressOp);
                }

                dataAddress.Store();

                var done = il.NewLabel();
                var isNotZero = il.NewLabel();

                dataAddress.Load();
                isNotZero.BranchIf(Condition.True, @short: true);

                il.Load(0);
                value.Store();
                done.Branch();

                isNotZero.Mark();

                dataAddress.Load();
                il.Math.Subtract(1);
                il.Convert.ToUInt16();
                dataAddress.Store();

                compiler.EmitLoadMemoryByte(dataAddress);
                value.Store();

                var checkForZero = il.NewLabel();

                if (compiler.Version < 4)
                {
                    value.Load();
                    il.Math.Shr(5);
                    il.Math.Add(1);
                    il.Convert.ToUInt8();
                    value.Store();
                    checkForZero.Branch();
                }

                var secondBranch = il.NewLabel();

                value.Load();
                il.Math.And(0x80);
                secondBranch.BranchIf(Condition.True, @short: true);

                value.Load();
                il.Math.Shr(6);
                il.Math.Add(1);
                il.Convert.ToUInt8();
                value.Store();
                checkForZero.Branch();

                secondBranch.Mark();

                value.Load();
                il.Math.And(0x3f);
                il.Convert.ToUInt8();
                value.Store();

                checkForZero.Mark();

                value.Load();
                done.BranchIf(Condition.True, @short: true);

                il.Load(64);
                value.Store();

                done.Mark();

                compiler.EmitStoreVariable(store, value, reuse: ReuseStoreVariable);
            }
        }

        public override bool CanReuseFirstOperand
        {
            get { return true; }
        }

        public override bool CanReuseStoreVariable
        {
            get { return true; }
        }
    }
}
