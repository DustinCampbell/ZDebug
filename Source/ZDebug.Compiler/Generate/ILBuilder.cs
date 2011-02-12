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

        private int opcodeCount;
        private int localCount;

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

        public int OpcodeCount
        {
            get { return opcodeCount; }
        }

        public int LocalCount
        {
            get { return localCount; }
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

        private void Emit(OpCode opcode)
        {
            il.Emit(opcode);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, int value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, string value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, Type value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, FieldInfo value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, MethodInfo value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, ConstructorInfo value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, Label value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private void Emit(OpCode opcode, LocalBuilder value)
        {
            il.Emit(opcode, value);
            opcodeCount++;
        }

        private LocalBuilder DeclareLocal(Type localType)
        {
            return il.DeclareLocal(localType);
            localCount++;
        }

        public void Duplicate()
        {
            Emit(OpCodes.Dup);
        }

        public void Pop()
        {
            Emit(OpCodes.Pop);
        }

        public void LoadArg(int index)
        {
            switch (index)
            {
                case 0:
                    Emit(OpCodes.Ldarg_0);
                    break;

                case 1:
                    Emit(OpCodes.Ldarg_1);
                    break;

                default:
                    throw new ZCompilerException("Unexpected argument index: " + index);
            }
        }

        public void Load(FieldInfo field)
        {
            Emit(OpCodes.Ldfld, field);
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
                    Emit(OpCodes.Ldc_I4, value);
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

    }
}
