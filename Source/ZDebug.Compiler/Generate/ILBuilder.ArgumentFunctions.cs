using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        public sealed class ArgumentFunctions : FunctionSet
        {
            public ArgumentFunctions(ILBuilder builder)
                : base(builder)
            {
            }

            public void LoadMachine()
            {
                builder.Emit(OpCodes.Ldarg_0);
            }

            public void LoadMemory()
            {
                builder.Emit(OpCodes.Ldarg_1);
            }

            public void LoadLocals()
            {
                builder.Emit(OpCodes.Ldarg_2);
            }

            public void LoadStack()
            {
                builder.Emit(OpCodes.Ldarg_3);
            }

            public void LoadSP()
            {
                builder.Emit(OpCodes.Ldarg_S, (byte)4);
            }

            public void StoreSP()
            {
                builder.Emit(OpCodes.Starg_S, (byte)4);
            }

            public void LoadCalls()
            {
                builder.Emit(OpCodes.Ldarg_S, (byte)5);
            }

            public void LoadArgCount()
            {
                builder.Emit(OpCodes.Ldarg_S, (byte)6);
            }
        }
    }
}
