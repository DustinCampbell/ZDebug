using System.Reflection;

namespace ZDebug.Compiler.Generate
{
    public delegate void CodeBuilder();

    public static class CodeBuilders
    {
        public static CodeBuilder Combine(this ILBuilder il, params CodeBuilder[] codeBuilders)
        {
            return () =>
            {
                foreach (var codeBuilder in codeBuilders)
                {
                    codeBuilder();
                }
            };
        }

        public static CodeBuilder GenerateLoadInstanceField(this ILBuilder il, FieldInfo field)
        {
            return () =>
            {
                il.Arguments.LoadMachine();
                il.Load(field);
            };
        }

        public static CodeBuilder GenerateLoadInstanceFieldAddress(this ILBuilder il, FieldInfo field)
        {
            return () =>
            {
                il.Arguments.LoadMachine();
                il.LoadAddress(field);
            };
        }

        public static CodeBuilder GenerateLoad(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Load(value);
            };
        }

        public static CodeBuilder GenerateLoad(this ILBuilder il, ILocal local)
        {
            return () =>
            {
                local.Load();
            };
        }

        public static CodeBuilder GenerateLoadAndBox(this ILBuilder il, ILocal local)
        {
            return () =>
            {
                local.LoadAndBox();
            };
        }

        public static CodeBuilder GenerateStore(this ILBuilder il, ILocal local)
        {
            return () =>
            {
                local.Store();
            };
        }

        public static CodeBuilder GenerateAdd(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.Add(value);
            };
        }

        public static CodeBuilder GenerateAdd(this ILBuilder il)
        {
            return () =>
            {
                il.Math.Add();
            };
        }

        public static CodeBuilder GenerateSubtract(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.Subtract(value);
            };
        }

        public static CodeBuilder GenerateSubtract(this ILBuilder il)
        {
            return () =>
            {
                il.Math.Subtract();
            };
        }

        public static CodeBuilder GenerateMultiply(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.Multiply(value);
            };
        }

        public static CodeBuilder GenerateMultiply(this ILBuilder il)
        {
            return () =>
            {
                il.Math.Multiply();
            };
        }

        public static CodeBuilder GenerateDivide(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.Divide(value);
            };
        }

        public static CodeBuilder GenerateDivide(this ILBuilder il)
        {
            return () =>
            {
                il.Math.Divide();
            };
        }

        public static CodeBuilder GenerateRemainder(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.Remainder(value);
            };
        }

        public static CodeBuilder GenerateRemainder(this ILBuilder il)
        {
            return () =>
            {
                il.Math.Remainder();
            };
        }

        public static CodeBuilder GenerateAnd(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.And(value);
            };
        }

        public static CodeBuilder GenerateAnd(this ILBuilder il)
        {
            return () =>
            {
                il.Math.And();
            };
        }

        public static CodeBuilder GenerateOr(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.Or(value);
            };
        }

        public static CodeBuilder GenerateOr(this ILBuilder il)
        {
            return () =>
            {
                il.Math.Or();
            };
        }

        public static CodeBuilder GenerateShr(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Math.Shr(value);
            };
        }

        public static CodeBuilder GenerateConvertToUInt8(this ILBuilder il)
        {
            return () =>
            {
                il.Convert.ToUInt8();
            };
        }

        public static CodeBuilder GenerateDuplicate(this ILBuilder il)
        {
            return () =>
            {
                il.Duplicate();
            };
        }

        public static CodeBuilder Generate(this ILBuilder il, CodeBuilder code)
        {
            return () =>
            {
                code();
            };
        }
    }
}
