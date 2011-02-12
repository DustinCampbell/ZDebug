using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        public sealed class CompareFunctions : FunctionSet
        {
            public CompareFunctions(ILBuilder builder) : base(builder)
            {
            }

            public void Equal()
            {
                builder.Emit(OpCodes.Ceq);
            }

            public void GreaterThan()
            {
                builder.Emit(OpCodes.Cgt);
            }

            public void LessThan()
            {
                builder.Emit(OpCodes.Clt);
            }

            public void AtLeast()
            {
                builder.Emit(OpCodes.Clt);
                builder.Load(0);
                this.Equal();
            }

            public void AtMost()
            {
                builder.Emit(OpCodes.Cgt);
                builder.Load(0);
                this.Equal();
            }
        }
    }
}
