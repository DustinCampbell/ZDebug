using System;
using System.Reflection.Emit;

namespace ZDebug.Compiler
{
    public static class ZCompiler
    {
        private static int count;

        public static Action Compile(ZRoutine routine)
        {
            var dm = new DynamicMethod("ZRoutine" + (++count).ToString(), typeof(void), new Type[0]);

            return (Action)dm.CreateDelegate(typeof(Action));
        }
    }
}
