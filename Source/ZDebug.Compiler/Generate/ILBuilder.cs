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
        }

        public void ConvertToUInt8()
        {
            il.Emit(OpCodes.Conv_U1);
        }

        public void ConvertToInt16()
        {
            il.Emit(OpCodes.Conv_I2);
        }

        public void ConvertToUInt16()
        {
            il.Emit(OpCodes.Conv_U2);
        }

        public void ConvertToInt32()
        {
            il.Emit(OpCodes.Conv_I4);
        }

        public void CompareEqual()
        {
            il.Emit(OpCodes.Ceq);
        }

        public void CompareGreaterThan()
        {
            il.Emit(OpCodes.Cgt);
        }

        public void CompareLessThan()
        {
            il.Emit(OpCodes.Clt);
        }

        public void CompareAtLeast()
        {
            il.Emit(OpCodes.Clt);
            LoadConstant(0);
            CompareEqual();
        }

        public void CompareAtMost()
        {
            il.Emit(OpCodes.Cgt);
            LoadConstant(0);
            CompareEqual();
        }

        public void Duplicate()
        {
            il.Emit(OpCodes.Dup);
        }

        public void Pop()
        {
            il.Emit(OpCodes.Pop);
        }

        public void LoadArgument(int index)
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

        public void LoadField(FieldInfo field)
        {
            il.Emit(OpCodes.Ldfld, field);
        }

        public void LoadConstant(string value)
        {
            il.Emit(OpCodes.Ldstr, value);
        }

        public void LoadConstant(bool value)
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

        public void LoadConstant(int value)
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
                default:
                    il.Emit(OpCodes.Ldc_I4, value);
                    break;
            }
        }

        public void Add()
        {
            il.Emit(OpCodes.Add);
        }

        public void Add(int value)
        {
            LoadConstant(value);
            Add();
        }

        public void Add(ILocal local)
        {
            local.Load();
            Add();
        }

        public void Subtract()
        {
            il.Emit(OpCodes.Sub);
        }

        public void Subtract(int value)
        {
            LoadConstant(value);
            Subtract();
        }

        public void Subtract(ILocal local)
        {
            local.Load();
            Subtract();
        }

        public void And()
        {
            il.Emit(OpCodes.And);
        }

        public void And(int value)
        {
            LoadConstant(value);
            And();
        }

        public void And(ILocal local)
        {
            local.Load();
            And();
        }

        public void Multiply()
        {
            il.Emit(OpCodes.Mul);
        }

        public void Multiply(int value)
        {
            LoadConstant(value);
            Multiply();
        }

        public void Multiply(ILocal local)
        {
            local.Load();
            Multiply();
        }

        public void Divide()
        {
            il.Emit(OpCodes.Div);
        }

        public void Divide(int value)
        {
            LoadConstant(value);
            Divide();
        }

        public void Divide(ILocal local)
        {
            local.Load();
            Divide();
        }

        public void Remainder()
        {
            il.Emit(OpCodes.Rem);
        }

        public void Remainder(int value)
        {
            LoadConstant(value);
            Remainder();
        }

        public void Remainder(ILocal local)
        {
            local.Load();
            Remainder();
        }

        public void Or()
        {
            il.Emit(OpCodes.Or);
        }

        public void Or(int value)
        {
            LoadConstant(value);
            Or();
        }

        public void Or(ILocal local)
        {
            local.Load();
            Or();
        }

        public void Not()
        {
            il.Emit(OpCodes.Not);
        }

        public void Shl()
        {
            il.Emit(OpCodes.Shl);
        }

        public void Shl(int value)
        {
            LoadConstant(value);
            Shl();
        }

        public void Shl(ILocal local)
        {
            local.Load();
            Shl();
        }

        public void Shr()
        {
            il.Emit(OpCodes.Shr);
        }

        public void Shr(int value)
        {
            LoadConstant(value);
            Shr();
        }

        public void Shr(ILocal local)
        {
            local.Load();
            Shr();
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
            LoadConstant(value);
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
            ConvertToInt32();

            Call(arrayCopy);
        }

        public void RuntimeError(string message)
        {
            LoadConstant(message);
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
