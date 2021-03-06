﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        private readonly ILGenerator il;
        private readonly Dictionary<Type, Stack<ILocal>> locals = new Dictionary<Type, Stack<ILocal>>();

        private readonly ArgumentFunctions arguments;
        private readonly CompareFunctions compare;
        private readonly ConvertFunctions convert;
        private readonly MathFunctions math;

        private int opcodeCount;
        private int localCount;

        private readonly static MethodInfo arrayCopy = Reflection<Array>.GetMethod(
            "Copy",
            Types.Array<Array, Array, int>(),
            instance: false);

        private readonly static MethodInfo mathMin = typeof(Math).GetMethod(
            name: "Min",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.Array<uint, uint>(),
            modifiers: null);

        private readonly static ConstructorInfo exceptionCtor = Reflection<ZMachineException>.GetConstructor(Types.Array<string>());

        public ILBuilder(ILGenerator il)
        {
            this.il = il;

            this.arguments = new ArgumentFunctions(this);
            this.compare = new CompareFunctions(this);
            this.convert = new ConvertFunctions(this);
            this.math = new MathFunctions(this);
        }

        public int OpcodeCount
        {
            get { return opcodeCount; }
        }

        public int LocalCount
        {
            get { return localCount; }
        }

        public int Size
        {
            get { return il.ILOffset; }
        }

        public ArgumentFunctions Arguments
        {
            get { return arguments; }
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

        internal void Emit(OpCode opcode)
        {
            il.Emit(opcode);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, byte value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, sbyte value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, int value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, string value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, Type value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, FieldInfo value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, MethodInfo value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, ConstructorInfo value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, Label value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        internal void Emit(OpCode opcode, LocalBuilder value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private LocalBuilder DeclareLocal(Type localType)
        {
            var loc = il.DeclareLocal(localType);
            localCount++;
            return loc;
        }

        public void Duplicate()
        {
            Emit(OpCodes.Dup);
        }

        public void Pop()
        {
            Emit(OpCodes.Pop);
        }

        public void Load(FieldInfo field, bool @volatile = false)
        {
            if (@volatile)
            {
                Emit(OpCodes.Volatile);
            }

            Emit(OpCodes.Ldfld, field);
        }

        public void LoadAddress(FieldInfo field, bool @volatile = false)
        {
            if (@volatile)
            {
                Emit(OpCodes.Volatile);
            }

            Emit(OpCodes.Ldflda, field);
        }

        public void Store(FieldInfo field, bool @volatile = false)
        {
            if (@volatile)
            {
                Emit(OpCodes.Volatile);
            }

            Emit(OpCodes.Stfld, field);
        }

        public void Load(string value)
        {
            Emit(OpCodes.Ldstr, value);
        }

        public void Load(bool value)
        {
            if (value)
            {
                Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                Emit(OpCodes.Ldc_I4_0);
            }
        }

        public void Load(int value)
        {
            switch (value)
            {
                case 0:
                    Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    Emit(OpCodes.Ldc_I4_8);
                    break;
                case -1:
                    Emit(OpCodes.Ldc_I4_M1);
                    break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        public void Call(MethodInfo method)
        {
            Emit(OpCodes.Call, method);
        }

        public void CallVirt(MethodInfo method)
        {
            Emit(OpCodes.Callvirt, method);
        }

        public void NewObject(ConstructorInfo constructor)
        {
            Emit(OpCodes.Newobj, constructor);
        }

        public void Return()
        {
            Emit(OpCodes.Ret);
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

            Emit(OpCodes.Call, arrayCopy);
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
            Emit(OpCodes.Newobj, exceptionCtor);
            Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, ILocal arg0)
        {
            FormatString(format, arg0);
            Emit(OpCodes.Newobj, exceptionCtor);
            Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, ILocal arg0, ILocal arg1)
        {
            FormatString(format, arg0, arg1);
            Emit(OpCodes.Newobj, exceptionCtor);
            Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, ILocal arg0, ILocal arg1, ILocal arg2)
        {
            FormatString(format, arg0, arg1, arg2);
            Emit(OpCodes.Newobj, exceptionCtor);
            Emit(OpCodes.Throw);
        }

        public void RuntimeError(string format, params ILocal[] args)
        {
            FormatString(format, args);
            Emit(OpCodes.Newobj, exceptionCtor);
            Emit(OpCodes.Throw);
        }

        public void ThrowException<T>() where T : Exception
        {
            var ctor = Reflection<T>.GetConstructor();

            Emit(OpCodes.Newobj, ctor);
            Emit(OpCodes.Throw);
        }

        public void ThrowException<T>(string message) where T : Exception
        {
            var ctor = Reflection<T>.GetConstructor(Types.Array<string>());

            Load(message);
            Emit(OpCodes.Newobj, ctor);
            Emit(OpCodes.Throw);
        }
    }
}
