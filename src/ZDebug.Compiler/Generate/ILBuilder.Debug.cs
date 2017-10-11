using System.Diagnostics;
using System.Reflection;
using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        private readonly static MethodInfo debugIndent = typeof(Debug).GetMethod(
            name: "Indent",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo debugUnindent = typeof(Debug).GetMethod(
            name: "Unindent",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo debugWriteLine = typeof(Debug).GetMethod(
            name: "WriteLine",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.Array<string>(),
            modifiers: null);

        [Conditional("LOG")]
        public void DebugWrite(string text)
        {
            Load(text);
            Call(debugWriteLine);
        }

        [Conditional("LOG")]
        public void DebugWrite(string format, ILocal arg0)
        {
            FormatString(format, arg0);
            Call(debugWriteLine);
        }

        [Conditional("LOG")]
        public void DebugWrite(string format, IRefLocal arg0)
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
        public void DebugWrite(string format, IRefLocal arg0, IRefLocal arg1)
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
        public void DebugWrite(string format, IRefLocal arg0, IRefLocal arg1, IRefLocal arg2)
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
