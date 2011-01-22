using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        public sealed class ConvertFunctions : FunctionSet
        {
            public ConvertFunctions(ILBuilder builder)
                : base(builder)
            {
            }

            public void ToUInt8()
            {
                builder.il.Emit(OpCodes.Conv_U1);
            }

            public void ToInt16()
            {
                builder.il.Emit(OpCodes.Conv_I2);
            }

            public void ToUInt16()
            {
                builder.il.Emit(OpCodes.Conv_U2);
            }

            public void ToInt32()
            {
                builder.il.Emit(OpCodes.Conv_I4);
            }
        }
    }
}
