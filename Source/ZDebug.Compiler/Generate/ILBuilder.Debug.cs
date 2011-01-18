using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Reflection.Emit;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        private readonly static MethodInfo debugIndent = typeof(Debug).GetMethod(
            name: "Indent",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[0],
            modifiers: null);

        private readonly static MethodInfo debugUnindent = typeof(Debug).GetMethod(
            name: "Unindent",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[0],
            modifiers: null);

        private readonly static MethodInfo debugWriteLine = typeof(Debug).GetMethod(
            name: "WriteLine",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: new Type[] { typeof(string) },
            modifiers: null);

        [Conditional("LOG")]
        public void DebugWrite(string text)
        {
            LoadConstant(text);
            Call(debugWriteLine);
        }

        [Conditional("LOG")]
        public void DebugWrite(string format, ILocal arg0)
        {
            FormatString(format, arg0);
            Call(debugWriteLine);
        }

        [Conditional("LOG")]
        public void DebugWrite(string format, ILocal arg0, ILocal arg1)
        {
            FormatString(format, arg0, arg1);
            Call(debugWriteLine);
        }

        [Conditional("LOG")]
        public void DebugWrite(string format, ILocal arg0, ILocal arg1, ILocal arg2)
        {
            FormatString(format, arg0, arg1, arg2);
            Call(debugWriteLine);
        }

        [Conditional("LOG")]
        public void DebugIndent()
        {
            Call(debugIndent);
        }

        [Conditional("LOG")]
        public void DebugUnindent()
        {
            Call(debugUnindent);
        }
    }
}
