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

            public void LoadThis()
            {
                builder.Emit(OpCodes.Ldarg_0);
            }

            public void LoadCalls()
            {
                builder.Emit(OpCodes.Ldarg_1);
            }

            public void LoadLocals()
            {
                builder.Emit(OpCodes.Ldarg_2);
            }

            public void LoadArgCount()
            {
                builder.Emit(OpCodes.Ldarg_3);
            }
        }
    }
}
