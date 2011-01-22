using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        private readonly ILGenerator il;
        private readonly Dictionary<Type, Stack<ILocal>> locals = new Dictionary<Type, Stack<ILocal>>();

        private readonly CompareFunctions compare;
        private readonly ConvertFunctions convert;
        private readonly MathFunctions math;

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

        public ILBuilder(ILGenerator il)
        {
            this.il = il;
            compare = new CompareFunctions(this);
            convert = new ConvertFunctions(this);
            math = new MathFunctions(this);
        }

        public CompareFunctions Compare
        {
            get { return compare; }
        }

        public ConvertFunctions Convert
        {
            get { return convert; }
        }

        public MathFunctions Math
        {
            get { return math; }
        }

        public void Duplicate()
        {
            il.Emit(OpCodes.Dup);
        }

        public void Pop()
        {
            il.Emit(OpCodes.Pop);
        }

        public void LoadArg(int index)
        {
            switch (index)
            {
                case 0:
                    il.Emit(OpCodes.Ldarg_0);
                    break;

                case 1:
                    il.Emit(OpCodes.Ldarg_1);
                    break;

                default:
                    throw new ZCompilerException("Unexpected argument index: " + index);
            }
        }

        public void Load(FieldInfo field)
        {
            il.Emit(OpCodes.Ldfld, field);
        }

        public void Load(string value)
        {
            il.Emit(OpCodes.Ldstr, value);
        }

        public void Load(bool value)
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

        public void Load(int value)
        {
            switch (value)
            {
                case 0:
                    il.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    il.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    il.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    il.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    il.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    il.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    il.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    il.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    il.Emit(OpCodes.Ldc_I4_8);
                    break;
                case -1:
                    il.Emit(OpCodes.Ldc_I4_M1);
                    break;
                default:
                    il.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        public void Call(MethodInfo method)
        {
            il.Emit(OpCodes.Call, method);
        }

        public void CallVirt(MethodInfo method)
        {
            il.Emit(OpCodes.Callvirt, method);
        }

        public void Return()
        {
            il.Emit(OpCodes.Ret);
        }

        public void Return(int value)
        {
            Load(value);
            Return();
        }

        public void CopyArray(IArrayLocal source, IArrayLocal destination, ILocal length)
        {
            source.Load();
            destination.Load();
            length.Load();

            il.Emit(OpCodes.Call, arrayCopy);
        }

        public void CopyArray(IArrayLocal source, IArrayLocal destination)
        {
            source.Load();
            destination.Load();

            source.LoadLength();
            destination.LoadLength();

            Call(mathMin);
            convert.ToInt32();

            Call(arrayCopy);
        }

        public void RuntimeError(string message)
        {
            Load(message);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, ILocal arg0)
        {
            FormatString(format, arg0);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, ILocal arg0, ILocal arg1)
        {
            FormatString(format, arg0, arg1);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, ILocal arg0, ILocal arg1, ILocal arg2)
        {
            FormatString(format, arg0, arg1, arg2);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, params ILocal[] args)
        {
            FormatString(format, args);
            il.Emit(OpCodes.Newobj, exceptionCtor);
            il.Emit(OpCodes.Throw);
        }

    }
}
