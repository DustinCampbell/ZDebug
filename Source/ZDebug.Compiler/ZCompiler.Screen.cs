using System;
using System.Reflection;
using System.Reflection.Emit;
using ZDebug.Compiler.Generate;
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
            screen.Load();
            il.LoadConstant(ch);
            il.CallVirt(print1);
        }

        private void PrintChar(ILocal ch)
        {
            il.DebugWrite("PrintChar: {0}", ch);

            screen.Load();
            ch.Load();
            il.CallVirt(print1);
        }

        private void PrintChar()
        {
            using (var ch = il.NewLocal<char>())
            {
                ch.Store();
                PrintChar(ch);
            }
        }

        private void PrintText(string text)
        {
            screen.Load();
            il.LoadConstant(text);
            il.CallVirt(print2);
        }

        private void PrintText(ILocal text)
        {
            screen.Load();
            text.Load();
            il.CallVirt(print2);
        }

        private void PrintText()
        {
            using (var text = il.NewLocal<string>())
            {
                text.Store();
                PrintText(text);
            }
        }
    }
}
