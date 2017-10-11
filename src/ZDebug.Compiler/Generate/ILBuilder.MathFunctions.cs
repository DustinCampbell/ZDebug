using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        public sealed class MathFunctions : FunctionSet
        {
            public MathFunctions(ILBuilder builder)
                : base(builder)
            {
            }

            public void Add()
            {
                builder.il.Emit(OpCodes.Add);
            }

            public void Add(int value)
            {
                builder.Load(value);
                Add();
            }

            public void Add(ILocal local)
            {
                local.Load();
                Add();
            }

            public void Subtract()
            {
                builder.il.Emit(OpCodes.Sub);
            }

            public void Subtract(int value)
            {
                builder.Load(value);
                Subtract();
            }

            public void Subtract(ILocal local)
            {
                local.Load();
                Subtract();
            }

            public void Multiply()
            {
                builder.il.Emit(OpCodes.Mul);
            }

            public void Multiply(int value)
            {
                builder.Load(value);
                Multiply();
            }

            public void Multiply(ILocal local)
            {
                local.Load();
                Multiply();
            }

            public void Divide()
            {
                builder.il.Emit(OpCodes.Div);
            }

            public void Divide(int value)
            {
                builder.Load(value);
                Divide();
            }

            public void Divide(ILocal local)
            {
                local.Load();
                Divide();
            }

            public void Remainder()
            {
                builder.il.Emit(OpCodes.Rem);
            }

            public void Remainder(int value)
            {
                builder.Load(value);
                Remainder();
            }

            public void Remainder(ILocal local)
            {
                local.Load();
                Remainder();
            }

            public void And()
            {
                builder.il.Emit(OpCodes.And);
            }

            public void And(int value)
            {
                builder.Load(value);
                And();
            }

            public void And(ILocal local)
            {
                local.Load();
                And();
            }

            public void Or()
            {
                builder.il.Emit(OpCodes.Or);
            }

            public void Or(int value)
            {
                builder.Load(value);
                Or();
            }

            public void Or(ILocal local)
            {
                local.Load();
                Or();
            }

            public void Not()
            {
                builder.il.Emit(OpCodes.Not);
            }

            public void Shl()
            {
                builder.il.Emit(OpCodes.Shl);
            }

            public void Shl(int value)
            {
                builder.Load(value);
                Shl();
            }

            public void Shl(ILocal local)
            {
                local.Load();
                Shl();
            }

            public void Shr()
            {
                builder.il.Emit(OpCodes.Shr);
            }

            public void Shr(int value)
            {
                builder.Load(value);
                Shr();
            }

            public void Shr(ILocal local)
            {
                local.Load();
                Shr();
            }

            public void Negate()
            {
                builder.il.Emit(OpCodes.Neg);
            }

            public void Negate(int value)
            {
                builder.Load(value);
                Negate();
            }

            public void Negate(ILocal local)
            {
                local.Load();
                Negate();
            }

            public void Increment(ILocal local, int count)
            {
                local.Load();
                Add(count);
                local.Store();
            }

            public void Increment(ILocal local)
            {
                Increment(local, 1);
            }
        }
    }
}
