using System;
using System.Reflection.Emit;

namespace ZDebug.Compiler.Tests.Utilities
{
    internal static class DynamicMethodHelpers
    {
        public static Delegate Create(Type delegateType, Action<ILGenerator> codeGenerator)
        {
            if (delegateType.BaseType != typeof(MulticastDelegate))
            {
                throw new ArgumentException("'delegateType' does not represent a valid delegate", "delegateType");
            }

            var invokeMethod = delegateType.GetMethod("Invoke");
            if (invokeMethod == null)
            {
                throw new ArgumentException("'delegateType' does not represent a valid delegate", "delegateType");
            }

            var returnType = invokeMethod.ReturnType;
            var parameterTypes = Array.ConvertAll(invokeMethod.GetParameters(), pi => pi.ParameterType);

            var dm = new DynamicMethod("TestMethod", returnType, parameterTypes);
            var il = dm.GetILGenerator();

            codeGenerator(il);

            il.Emit(OpCodes.Ret);

            return dm.CreateDelegate(delegateType);
        }

    }
}
