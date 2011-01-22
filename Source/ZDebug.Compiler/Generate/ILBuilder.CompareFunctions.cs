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
                builder.il.Emit(OpCodes.Ceq);
            }

            public void GreaterThan()
            {
                builder.il.Emit(OpCodes.Cgt);
            }

            public void LessThan()
            {
                builder.il.Emit(OpCodes.Clt);
            }

            public void AtLeast()
            {
                builder.il.Emit(OpCodes.Clt);
                builder.Load(0);
                this.Equal();
            }

            public void AtMost()
            {
                builder.il.Emit(OpCodes.Cgt);
                builder.Load(0);
                this.Equal();
            }
        }
    }
}
