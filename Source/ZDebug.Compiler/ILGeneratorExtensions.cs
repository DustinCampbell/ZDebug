using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

namespace ZDebug.Compiler
{
    internal static partial class ILGeneratorExtensions
    {
        private readonly static MethodInfo stringFormat1 = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[] { typeof(string), typeof(object) },
            modifiers: null);

        private readonly static MethodInfo stringFormat2 = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[] { typeof(string), typeof(object), typeof(object) },
            modifiers: null);

        private readonly static MethodInfo stringFormat3 = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[] { typeof(string), typeof(object), typeof(object), typeof(object) },
            modifiers: null);

        private readonly static MethodInfo stringFormatAny = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[] { typeof(string), typeof(object[]) },
            modifiers: null);

        private readonly static MethodInfo arrayCopy = typeof(Array).GetMethod(
            name: "Copy",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[] { typeof(Array), typeof(Array), typeof(int) },
            modifiers: null);

        private readonly static MethodInfo mathMin = typeof(Math).GetMethod(
            name: "Min",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[] { typeof(uint), typeof(uint) },
            modifiers: null);

        private readonly static ConstructorInfo exceptionCtor = typeof(ZMachineException).GetConstructor(
            types: new Type[] { typeof(string) });

        public static LocalBuilder DeclareLocal<T>(this ILGenerator il)
        {
            return il.DeclareLocal(typeof(T));
        }
        
        public static LocalBuilder DeclareArrayLocal<T>(this ILGenerator il, int length)
        {
            var loc = il.DeclareLocal(typeof(T[]));
            il.Emit(OpCodes.Ldc_I4, length);
            il.Emit(OpCodes.Newarr, typeof(T));
            il.Emit(OpCodes.Stloc, loc);

            return loc;
        }

        public static LocalBuilder DeclareLocal(this ILGenerator il, string value)
        {
            var loc = il.DeclareLocal(typeof(string));
            il.Emit(OpCodes.Ldstr, value);
            il.Emit(OpCodes.Stloc, loc);

            return loc;
        }

        public static LocalBuilder DeclareLocal(this ILGenerator il, ushort value)
        {
            var loc = il.DeclareLocal(typeof(ushort));
            il.Emit(OpCodes.Ldc_I4, value);
            il.Emit(OpCodes.Stloc, loc);

            return loc;
        }

        public static LocalBuilder DeclareLocal(this ILGenerator il, int value)
        {
            var loc = il.DeclareLocal(typeof(int));
            il.Emit(OpCodes.Ldc_I4, value);
            il.Emit(OpCodes.Stloc, loc);

            return loc;
        }

        public static LocalBuilder DeclareLocal(this ILGenerator il, bool value)
        {
            var loc = il.DeclareLocal(typeof(bool));
            il.LoadBool(value);

            il.Emit(OpCodes.Stloc, loc);

            return loc;
        }

        public static LocalBuilder DeclareLocal(this ILGenerator il, ushort[] values)
        {
            var loc = il.DeclareArrayLocal<ushort>(values.Length);

            for (int i = 0; i < values.Length; i++)
            {
                il.Emit(OpCodes.Ldloc, loc);
                il.Emit(OpCodes.Ldc_I4, i);
                il.Emit(OpCodes.Ldc_I4, values[i]);

                il.Emit(OpCodes.Stelem_I2);
            }

            return loc;
        }

        public static void LoadAndBox(this ILGenerator il, LocalBuilder loc)
        {
            il.Emit(OpCodes.Ldloc, loc);
            if (loc.LocalType.IsValueType)
            {
                il.Emit(OpCodes.Box, loc.LocalType);
            }
        }

        public static void LoadBool(this ILGenerator il, bool value)
        {
            if (value)
            {
                il.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4_0);
            }
        }

        public static void FormatString(this ILGenerator il, string format, LocalBuilder arg0)
        {
            il.Emit(OpCodes.Ldstr, format);
            il.LoadAndBox(arg0);

            il.Emit(OpCodes.Call, stringFormat1);
        }

        public static void FormatString(this ILGenerator il, string format, LocalBuilder arg0, LocalBuilder arg1)
        {
            il.Emit(OpCodes.Ldstr, format);
            il.LoadAndBox(arg0);
            il.LoadAndBox(arg1);

            il.Emit(OpCodes.Call, stringFormat2);
        }

        public static void FormatString(this ILGenerator il, string format, LocalBuilder arg0, LocalBuilder arg1, LocalBuilder arg2)
        {
            il.Emit(OpCodes.Ldstr, format);
            il.LoadAndBox(arg0);
            il.LoadAndBox(arg1);
            il.LoadAndBox(arg2);

            il.Emit(OpCodes.Call, stringFormat3);
        }

        public static void FormatString(this ILGenerator il, string format, params LocalBuilder[] args)
        {
            var locArgs = il.DeclareLocal(typeof(object[]));
            il.Emit(OpCodes.Ldc_I4, args.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            il.Emit(OpCodes.Stloc, locArgs);

            for (int i = 0; i < args.Length; i++)
            {
                il.Emit(OpCodes.Ldloc, locArgs);
                il.Emit(OpCodes.Ldc_I4, i);
                il.LoadAndBox(args[i]);

                il.Emit(OpCodes.Stelem_Ref);
            }

            il.Emit(OpCodes.Ldstr, format);
            il.Emit(OpCodes.Ldloc, locArgs);
            il.Emit(OpCodes.Call, stringFormatAny);
        }

        public static void CopyArray(this ILGenerator il, LocalBuilder source, LocalBuilder destination, LocalBuilder length)
        {
            il.Emit(OpCodes.Ldloc, source);
            il.Emit(OpCodes.Ldloc, destination);
            il.Emit(OpCodes.Ldloc, length);

            il.Emit(OpCodes.Call, arrayCopy);
        }

        public static void CopyArray(this ILGenerator il, LocalBuilder source, LocalBuilder destination)
        {
            il.Emit(OpCodes.Ldloc, source);
            il.Emit(OpCodes.Ldloc, destination);

            il.Emit(OpCodes.Ldloc, source);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Ldloc, destination);
            il.Emit(OpCodes.Ldlen);
            il.Emit(OpCodes.Call, mathMin);
            il.Emit(OpCodes.Conv_I4);

            il.Emit(OpCodes.Call, arrayCopy);
        }

        public static void ThrowException(this ILGenerator il, string message)
        {
            il.Emit(OpCodes.Ldstr, message);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public static void ThrowException(this ILGenerator il, string format, LocalBuilder arg0)
        {
            il.FormatString(format, arg0);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public static void ThrowException(this ILGenerator il, string format, LocalBuilder arg0, LocalBuilder arg1)
        {
            il.FormatString(format, arg0, arg1);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public static void ThrowException(this ILGenerator il, string format, LocalBuilder arg0, LocalBuilder arg1, LocalBuilder arg2)
        {
            il.FormatString(format, arg0, arg1, arg2);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public static void ThrowException(this ILGenerator il, string format, params LocalBuilder[] args)
        {
            il.FormatString(format, args);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }
    }
}
