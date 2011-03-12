using System.Reflection;
using ZDebug.Compiler.Utilities;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        private readonly static MethodInfo stringFormat1 = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.Two<string, object>(),
            modifiers: null);

        private readonly static MethodInfo stringFormat2 = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.Three<string, object, object>(),
            modifiers: null);

        private readonly static MethodInfo stringFormat3 = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.Four<string, object, object, object>(),
            modifiers: null);

        private readonly static MethodInfo stringFormatAny = typeof(string).GetMethod(
            name: "Format",
            bindingAttr: BindingFlags.Public | BindingFlags.Static,
            binder: null,
            types: Types.Two<string, object[]>(),
            modifiers: null);

        public void FormatString(string format, ILocal arg0)
        {
            Load(format);
            arg0.LoadAndBox();

            Call(stringFormat1);
        }

        public void FormatString(string format, ILocal arg0, ILocal arg1)
        {
            Load(format);
            arg0.LoadAndBox();
            arg1.LoadAndBox();

            Call(stringFormat2);
        }

        public void FormatString(string format, ILocal arg0, ILocal arg1, ILocal arg2)
        {
            Load(format);
            arg0.LoadAndBox();
            arg1.LoadAndBox();
            arg2.LoadAndBox();

            Call(stringFormat3);
        }

        public void FormatString(string format, params ILocal[] args)
        {
            var locArgs = NewArrayLocal<object>(args.Length);

            for (int i = 0; i < args.Length; i++)
            {
                locArgs.StoreElement(
                    loadIndex: this.GenerateLoad(i),
                    loadValue: this.GenerateLoadAndBox(args[i]));
            }

            Load(format);
            locArgs.Load();

            Call(stringFormatAny);
        }
    }
}
