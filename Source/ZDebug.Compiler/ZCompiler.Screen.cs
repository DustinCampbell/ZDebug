using System.Reflection;
using ZDebug.Compiler.Generate;
using ZDebug.Compiler.Utilities;
using ZDebug.Core.Execution;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private readonly static MethodInfo print1 = typeof(ZMachine.OutputStreams).GetMethod(
            name: "Print",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<char>(),
            modifiers: null);

        private readonly static MethodInfo print2 = typeof(ZMachine.OutputStreams).GetMethod(
            name: "Print",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<string>(),
            modifiers: null);

        private readonly static MethodInfo selectScreenStream = typeof(ZMachine.OutputStreams).GetMethod(
            name: "SelectScreenStream",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo deselectScreenStream = typeof(ZMachine.OutputStreams).GetMethod(
            name: "DeselectScreenStream",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo selectTranscriptStream = typeof(ZMachine.OutputStreams).GetMethod(
            name: "SelectTranscriptStream",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo deselectTranscriptStream = typeof(ZMachine.OutputStreams).GetMethod(
            name: "DeselectTranscriptStream",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo selectMemoryStream = typeof(ZMachine.OutputStreams).GetMethod(
            name: "SelectMemoryStream",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<int>(),
            modifiers: null);

        private readonly static MethodInfo deselectMemoryStream = typeof(ZMachine.OutputStreams).GetMethod(
            name: "DeselectMemoryStream",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo setTextStyle = typeof(IScreen).GetMethod(
            name: "SetTextStyle",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<ZTextStyle>(),
            modifiers: null);

        private readonly static MethodInfo clear = typeof(IScreen).GetMethod(
            name: "Clear",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<int>(),
            modifiers: null);

        private readonly static MethodInfo clearAll = typeof(IScreen).GetMethod(
            name: "ClearAll",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<bool>(),
            modifiers: null);

        private readonly static MethodInfo split = typeof(IScreen).GetMethod(
            name: "Split",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<int>(),
            modifiers: null);

        private readonly static MethodInfo setWindow = typeof(IScreen).GetMethod(
            name: "SetWindow",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<int>(),
            modifiers: null);

        private readonly static MethodInfo showStatus = typeof(IScreen).GetMethod(
            name: "ShowStatus",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.None,
            modifiers: null);

        private readonly static MethodInfo setForegroundColor = typeof(IScreen).GetMethod(
            name: "SetForegroundColor",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<ZColor>(),
            modifiers: null);

        private readonly static MethodInfo setBackgroundColor = typeof(IScreen).GetMethod(
            name: "SetBackgroundColor",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.One<ZColor>(),
            modifiers: null);

        private readonly static MethodInfo setCursor = typeof(IScreen).GetMethod(
            name: "SetCursor",
            bindingAttr: BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: Types.Two<int, int>(),
            modifiers: null);

        private void PrintChar(char ch)
        {
            outputStreams.Load();
            il.Load(ch);
            il.CallVirt(print1);
        }

        private void PrintChar(ILocal ch)
        {
            il.DebugWrite("PrintChar: {0}", ch);

            outputStreams.Load();
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
            outputStreams.Load();
            il.Load(text);
            il.CallVirt(print2);
        }

        private void PrintText(ILocal text)
        {
            outputStreams.Load();
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

        private void SetTextStyle()
        {
            using (var style = il.NewLocal<ZTextStyle>())
            {
                style.Store();

                screen.Load();
                style.Load();
                il.CallVirt(setTextStyle);
            }
        }

        private void EraseWindow()
        {
            using (var window = il.NewLocal<short>())
            {
                il.Convert.ToInt16();
                window.Store();

                var clearAllWindows = il.NewLabel();
                var done = il.NewLabel();

                window.Load();
                il.Load(0);
                clearAllWindows.BranchIf(Condition.LessThan, @short: true);

                screen.Load();
                window.Load();
                il.CallVirt(clear);

                done.Branch(@short: true);

                clearAllWindows.Mark();

                screen.Load();

                window.Load();
                il.Load(-1);
                il.Compare.Equal();

                il.CallVirt(clearAll);

                done.Mark();
            }
        }

        private void SplitWindow()
        {
            using (var lines = il.NewLocal<int>())
            {
                lines.Store();

                screen.Load();
                lines.Load();
                il.CallVirt(split);
            }
        }

        private void SetWindow()
        {
            using (var window = il.NewLocal<int>())
            {
                window.Store();

                screen.Load();
                window.Load();
                il.CallVirt(setWindow);
            }
        }

        private void ShowStatus()
        {
            screen.Load();
            il.CallVirt(showStatus);
        }

        private void SetColor(ILocal foreground, ILocal background)
        {
            var next = il.NewLabel();
            foreground.Load();
            next.BranchIf(Condition.False, @short: true);

            screen.Load();
            foreground.Load();
            il.Call(setForegroundColor);

            next.Mark();

            next = il.NewLabel();
            background.Load();
            next.BranchIf(Condition.False, @short: true);

            screen.Load();
            background.Load();
            il.Call(setBackgroundColor);

            next.Mark();
        }

        private void SetCursor(ILocal line, ILocal column)
        {
            screen.Load();
            line.Load();
            il.Math.Subtract(1);
            column.Load();
            il.Math.Subtract(1);
            il.Call(setCursor);
        }

        private void SelectScreenStream()
        {
            outputStreams.Load();
            il.Call(selectScreenStream);
        }

        private void DeselectScreenStream()
        {
            outputStreams.Load();
            il.Call(deselectScreenStream);
        }

        private void SelectTranscriptStream()
        {
            outputStreams.Load();
            il.Call(selectTranscriptStream);
        }

        private void DeselectTranscriptStream()
        {
            outputStreams.Load();
            il.Call(deselectTranscriptStream);
        }

        private void SelectMemoryStream(ILocal address)
        {
            outputStreams.Load();
            address.Load();
            il.Call(selectMemoryStream);
        }

        private void DeselectMemoryStream()
        {
            outputStreams.Load();
            il.Call(deselectMemoryStream);
        }
    }
}
