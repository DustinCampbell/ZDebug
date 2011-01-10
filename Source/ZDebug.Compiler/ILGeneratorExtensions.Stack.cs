using System.Reflection.Emit;

namespace ZDebug.Compiler
{
    internal static partial class ILGeneratorExtensions
    {
        public static void CheckStackEmpty(this ILGenerator il, LocalBuilder sp)
        {
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);

            var ok = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, ok);
            il.ThrowException("Stack is empty.");

            il.MarkLabel(ok);
        }

        public static void CheckStackFull(this ILGenerator il, LocalBuilder sp)
        {
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4, ZCompiler.STACK_SIZE);
            il.Emit(OpCodes.Ceq);

            var ok = il.DefineLabel();
            il.Emit(OpCodes.Brfalse_S, ok);
            il.ThrowException("Stack is full.");

            il.MarkLabel(ok);
        }

        public static void PopStack(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder result)
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

        public static void PeekStack(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder result)
        {
            il.CheckStackEmpty(sp);

            il.Emit(OpCodes.Ldloc, stack);
            il.Emit(OpCodes.Ldloc, sp);
            il.Emit(OpCodes.Ldc_I4_1);
            il.Emit(OpCodes.Sub);
            il.Emit(OpCodes.Ldelem_U2);
            il.Emit(OpCodes.Stloc, result);
        }

        public static void PushStack(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder value)
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

        public static void SetStackTop(this ILGenerator il, LocalBuilder stack, LocalBuilder sp, LocalBuilder value)
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
