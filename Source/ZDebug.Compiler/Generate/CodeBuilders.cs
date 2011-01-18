using System.Reflection;
using System.Reflection.Emit;
using System;

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

        public static CodeBuilder GenerateLoadArgument(this ILBuilder il, int index)
        {
            return () =>
            {
                il.LoadArgument(index);
            };
        }

        public static CodeBuilder GenerateLoadInstanceField(this ILBuilder il, FieldInfo field)
        {
            return () =>
            {
                il.LoadArgument(0);
                il.LoadField(field);
            };
        }

        public static CodeBuilder GenerateLoadConstant(this ILBuilder il, int value)
        {
            return () =>
            {
                il.LoadConstant(value);
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
                il.Add(value);
            };
        }

        public static CodeBuilder GenerateAdd(this ILBuilder il)
        {
            return () =>
            {
                il.Add();
            };
        }

        public static CodeBuilder GenerateSubtract(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Subtract(value);
            };
        }

        public static CodeBuilder GenerateSubtract(this ILBuilder il)
        {
            return () =>
            {
                il.Subtract();
            };
        }

        public static CodeBuilder GenerateMultiply(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Multiply(value);
            };
        }

        public static CodeBuilder GenerateMultiply(this ILBuilder il)
        {
            return () =>
            {
                il.Multiply();
            };
        }

        public static CodeBuilder GenerateDivide(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Divide(value);
            };
        }

        public static CodeBuilder GenerateDivide(this ILBuilder il)
        {
            return () =>
            {
                il.Divide();
            };
        }

        public static CodeBuilder GenerateRemainder(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Remainder(value);
            };
        }

        public static CodeBuilder GenerateRemainder(this ILBuilder il)
        {
            return () =>
            {
                il.Remainder();
            };
        }

        public static CodeBuilder GenerateAnd(this ILBuilder il, int value)
        {
            return () =>
            {
                il.And(value);
            };
        }

        public static CodeBuilder GenerateAnd(this ILBuilder il)
        {
            return () =>
            {
                il.And();
            };
        }

        public static CodeBuilder GenerateOr(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Or(value);
            };
        }

        public static CodeBuilder GenerateOr(this ILBuilder il)
        {
            return () =>
            {
                il.Or();
            };
        }

        public static CodeBuilder GenerateShr(this ILBuilder il, int value)
        {
            return () =>
            {
                il.Shr(value);
            };
        }

        public static CodeBuilder GenerateConvertToUInt8(this ILBuilder il)
        {
            return () =>
            {
                il.ConvertToUInt8();
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
