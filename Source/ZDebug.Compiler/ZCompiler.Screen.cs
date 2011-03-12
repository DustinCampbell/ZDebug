using ZDebug.Compiler.Generate;
using ZDebug.Compiler.Utilities;
using ZDebug.Core.Execution;

namespace ZDebug.Compiler
{
    public partial class ZCompiler
    {
        private void PrintChar(char ch)
        {
            outputStreams.Load();
            il.Load(ch);

            var print = Reflection<ZMachine.OutputStreams>.GetMethod("Print", Types.One<char>());
            il.CallVirt(print);
        }

        private void PrintChar(ILocal ch)
        {
            il.DebugWrite("PrintChar: {0}", ch);

            outputStreams.Load();
            ch.Load();

            var print = Reflection<ZMachine.OutputStreams>.GetMethod("Print", Types.One<char>());
            il.CallVirt(print);
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

            var print = Reflection<ZMachine.OutputStreams>.GetMethod("Print", Types.One<string>());
            il.CallVirt(print);
        }

        private void PrintText(ILocal text)
        {
            outputStreams.Load();
            text.Load();

            var print = Reflection<ZMachine.OutputStreams>.GetMethod("Print", Types.One<string>());
            il.CallVirt(print);
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

                var setTextStyle = Reflection<IScreen>.GetMethod("SetTextStyle", Types.One<ZTextStyle>());
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

                var clear = Reflection<IScreen>.GetMethod("Clear", Types.One<int>());
                il.CallVirt(clear);

                done.Branch(@short: true);

                clearAllWindows.Mark();

                screen.Load();

                window.Load();
                il.Load(-1);
                il.Compare.Equal();

                var clearAll = Reflection<IScreen>.GetMethod("ClearAll", Types.One<bool>());
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


                var split = Reflection<IScreen>.GetMethod("Split", Types.One<int>());
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

                var setWindow = Reflection<IScreen>.GetMethod("SetWindow", Types.One<int>());
                il.CallVirt(setWindow);
            }
        }

        private void ShowStatus()
        {
            screen.Load();

            var showStatus = Reflection<IScreen>.GetMethod("ShowStatus", Types.None);
            il.CallVirt(showStatus);
        }

        private void SetColor(ILocal foreground, ILocal background)
        {
            var next = il.NewLabel();
            foreground.Load();
            next.BranchIf(Condition.False, @short: true);

            screen.Load();
            foreground.Load();

            var setForegroundColor = Reflection<IScreen>.GetMethod("SetForegroundColor", Types.One<ZColor>());
            il.Call(setForegroundColor);

            next.Mark();

            next = il.NewLabel();
            background.Load();
            next.BranchIf(Condition.False, @short: true);

            screen.Load();
            background.Load();

            var setBackgroundColor = Reflection<IScreen>.GetMethod("SetBackgroundColor", Types.One<ZColor>());
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

            var setCursor = Reflection<IScreen>.GetMethod("SetCursor", Types.Two<int, int>());
            il.Call(setCursor);
        }

        private void SelectScreenStream()
        {
            outputStreams.Load();

            var selectScreenStream = Reflection<ZMachine.OutputStreams>.GetMethod("SelectScreenStream", Types.None);
            il.Call(selectScreenStream);
        }

        private void DeselectScreenStream()
        {
            outputStreams.Load();

            var deselectScreenStream = Reflection<ZMachine.OutputStreams>.GetMethod("DeselectScreenStream", Types.None);
            il.Call(deselectScreenStream);
        }

        private void SelectTranscriptStream()
        {
            outputStreams.Load();

            var selectTranscriptStream = Reflection<ZMachine.OutputStreams>.GetMethod("SelectTranscriptStream", Types.None);
            il.Call(selectTranscriptStream);
        }

        private void DeselectTranscriptStream()
        {
            outputStreams.Load();

            var deselectTranscriptStream = Reflection<ZMachine.OutputStreams>.GetMethod("DeselectTranscriptStream", Types.None);
            il.Call(deselectTranscriptStream);
        }

        private void SelectMemoryStream(ILocal address)
        {
            outputStreams.Load();
            address.Load();

            var selectMemoryStream = Reflection<ZMachine.OutputStreams>.GetMethod("SelectMemoryStream", Types.One<int>());
            il.Call(selectMemoryStream);
        }

        private void DeselectMemoryStream()
        {
            outputStreams.Load();

            var deselectMemoryStream = Reflection<ZMachine.OutputStreams>.GetMethod("DeselectMemoryStream", Types.None);
            il.Call(deselectMemoryStream);
        }
    }
}
