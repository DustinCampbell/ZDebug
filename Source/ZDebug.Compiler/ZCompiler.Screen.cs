using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Reflection;
using ZDebug.Core.Execution;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private readonly static MethodInfo print1 = typeof(IOutputStream).GetMethod(
            name: "Print",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(char) },
            modifiers: null);

        private readonly static MethodInfo print2 = typeof(IOutputStream).GetMethod(
            name: "Print",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: new Type[] { typeof(string) },
            modifiers: null);

        private void PrintChar(char ch)
        {
            il.Emit(OpCodes.Ldloc, screen);
            il.Emit(OpCodes.Ldc_I4, ch);
            il.Emit(OpCodes.Callvirt, print1);
        }

        private void PrintChar(LocalBuilder ch)
        {
            il.DebugWrite("PrintChar: {0}", ch);

            il.Emit(OpCodes.Ldloc, screen);
            il.Emit(OpCodes.Ldloc, ch);
            il.Emit(OpCodes.Callvirt, print1);
        }

        private void PrintChar()
        {
            using (var ch = localManager.AllocateTemp<char>())
            {
                il.Emit(OpCodes.Stloc, ch);
                PrintChar(ch);
            }
        }

        private void PrintText(string text)
        {
            il.Emit(OpCodes.Ldloc, screen);
            il.Emit(OpCodes.Ldstr, text);
            il.Emit(OpCodes.Callvirt, print2);
        }

        private void PrintText(LocalBuilder text)
        {
            il.Emit(OpCodes.Ldloc, screen);
            il.Emit(OpCodes.Ldloc, text);
            il.Emit(OpCodes.Callvirt, print2);
        }

        private void PrintText()
        {
            using (var text = localManager.AllocateTemp<string>())
            {
                il.Emit(OpCodes.Stloc, text);
                PrintText(text);
            }
        }
    }
}
