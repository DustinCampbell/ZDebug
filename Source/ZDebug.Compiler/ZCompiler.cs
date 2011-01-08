using System;
using System.Reflection.Emit;
using System.Collections.Generic;
using ZDebug.Core.Instructions;
using System.Reflection;

namespace ZDebug.Compiler
{
    public static class ZCompiler
    {
        internal const int STACK_SIZE = 1024;

        private readonly static FieldInfo memory = typeof(ZMachine).GetField("memory", BindingFlags.NonPublic | BindingFlags.Instance);

        private static string GetName(ZRoutine routine)
        {
            return "ZRoutine_" + routine.Address.ToString("x4");
        }

        public static Action Compile(ZRoutine routine, ZMachine machine)
        {
            var dm = new DynamicMethod(GetName(routine), typeof(void), new Type[] { typeof(ZMachine) });
            var il = dm.GetILGenerator();

            // First pass: gather branches and labels
            var addressToLabelMap = new Dictionary<int, Label>();
            foreach (var i in routine.Instructions)
            {
                if (i.HasBranch && i.Branch.Kind == BranchKind.Address)
                {
                    var address = i.Address + i.Length + i.Branch.Offset - 2;
                    addressToLabelMap.Add(address, il.DefineLabel());
                }
                else if (i.Opcode.IsJump)
                {
                    var address = i.Address + i.Length + (short)i.Operands[0].Value - 2;
                    addressToLabelMap.Add(address, il.DefineLabel());
                }
            }

            // Second pass: determine whether local stack is used
            var usesStack = false;
            foreach (var i in routine.Instructions)
            {
                usesStack = i.UsesStack();
                if (usesStack)
                {
                    break;
                }
            }

            // Create local variables for local stack, sp and this routine's locals
            var stack = usesStack ? il.DeclareArrayLocal<ushort>(STACK_SIZE) : null;
            var sp = usesStack ? il.DeclareLocal(0) : null;

            int localCount = routine.Locals.Length;
            var locals = new LocalBuilder[localCount];
            for (int i = 0; i < localCount; i++)
            {
                locals[i] = il.DeclareLocal(typeof(ushort));
            }

            // Initalize locals


            il.Emit(OpCodes.Ret);

            return (Action)dm.CreateDelegate(typeof(Action), machine);
        }

        internal static void CheckStackEmpty(this ILGenerator il, LocalBuilder sp)
        {
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);

            var ok = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, ok);
            il.ThrowException("Stack is empty.");

            il.MarkLabel(ok);
        }

        internal static void CheckStackFull(this ILGenerator il, LocalBuilder sp)
        {
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4, STACK_SIZE);
            il.Emit(OpCodes.Ceq);

            var ok = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, ok);
            il.ThrowException("Stack is full.");

            il.MarkLabel(ok);
        }

        internal static void PopStack(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder result)
        {
            il.CheckStackEmpty(sp);

            il.Emit(OpCodes.Ldloc, stack);

            // decrement sp
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Stloc, sp);

            // store item from stack in result
            il.Emit(OpCodes.Ldelem_U2);
            il.Emit(OpCodes.Stloc, result);
        }

        internal static void PeekStack(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder result)
        {
            il.CheckStackEmpty(sp);

            il.Emit(OpCodes.Ldloc, stack);
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Ldelem_U2);
            il.Emit(OpCodes.Stloc, result);
        }

        internal static void PushStack(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder value)
        {
            il.CheckStackFull(sp);

            // store value in stack
            il.Emit(OpCodes.Ldloc, stack);
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Stelem_I2);

            // increment sp
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, sp);
        }

        internal static void SetStackTop(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder value)
        {
            il.CheckStackEmpty(sp);

            il.Emit(OpCodes.Ldloc, stack);
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Ldloc, value);
            il.Emit(OpCodes.Stelem_I2);
        }
    }
}
