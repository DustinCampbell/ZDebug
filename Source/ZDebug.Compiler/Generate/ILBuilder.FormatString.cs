using ZDebug.Core.Utilities;

namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        public void FormatString(string format, ILocal arg0)
        {
            var stringFormat = Reflection<string>.GetMethod(
                "Format",
                Types.Two<string, object>(),
                instance: false);

            Load(format);
            arg0.LoadAndBox();

            Call(stringFormat);
        }

        public void FormatString(string format, IRefLocal arg0)
        {
            var stringFormat = Reflection<string>.GetMethod(
                "Format",
                Types.Two<string, object>(),
                instance: false);

            Load(format);
            arg0.Load();
            arg0.LoadIndirectValueAndBox();

            Call(stringFormat);
        }

        public void FormatString(string format, ILocal arg0, ILocal arg1)
        {
            var stringFormat = Reflection<string>.GetMethod(
                "Format",
                Types.Three<string, object, object>(),
                instance: false);

            Load(format);
            arg0.LoadAndBox();
            arg1.LoadAndBox();

            Call(stringFormat);
        }

        public void FormatString(string format, IRefLocal arg0, IRefLocal arg1)
        {
            var stringFormat = Reflection<string>.GetMethod(
                "Format",
                Types.Three<string, object, object>(),
                instance: false);

            Load(format);
            arg0.Load();
            arg0.LoadIndirectValueAndBox();
            arg1.Load();
            arg1.LoadIndirectValueAndBox();

            Call(stringFormat);
        }

        public void FormatString(string format, ILocal arg0, ILocal arg1, ILocal arg2)
        {
            var stringFormat = Reflection<string>.GetMethod(
                "Format",
                Types.Four<string, object, object, object>(),
                instance: false);

            Load(format);
            arg0.LoadAndBox();
            arg1.LoadAndBox();
            arg2.LoadAndBox();

            Call(stringFormat);
        }

        public void FormatString(string format, IRefLocal arg0, IRefLocal arg1, IRefLocal arg2)
        {
            var stringFormat = Reflection<string>.GetMethod(
                "Format",
                Types.Four<string, object, object, object>(),
                instance: false);

            Load(format);
            arg0.Load();
            arg0.LoadIndirectValueAndBox();
            arg1.Load();
            arg1.LoadIndirectValueAndBox();
            arg2.Load();
            arg2.LoadIndirectValueAndBox();

            Call(stringFormat);
        }

        public void FormatString(string format, params ILocal[] args)
        {
            var stringFormat = Reflection<string>.GetMethod(
                "Format",
                Types.Two<string, object[]>(),
                instance: false);

            var locArgs = NewArrayLocal<object>(args.Length);

            for (int i = 0; i < args.Length; i++)
            {
                locArgs.StoreElement(
                    indexLoader: () => Load(i),
                    valueLoader: () => args[i].LoadAndBox());
            }

            Load(format);
            locArgs.Load();

            Call(stringFormat);
        }
    }
}
