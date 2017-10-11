using ZDebug.Core.Interpreter;

namespace ZDebug.Compiler.Tests.Mocks
{
    internal sealed class MockInterpreter : IInterpreter
    {
        public InterpreterTarget Target
        {
            get { return InterpreterTarget.IBMPC; }
        }

        public byte Version
        {
            get { return (byte)'A'; }
        }

        public byte StandardRevisionMajorVersion
        {
            get { return 1; }
        }

        public byte StandardRevisionMinorVersion
        {
            get { return 0; }
        }

        public bool SupportsStatusLine
        {
            get { return true; }
        }

        public bool SupportsScreenSplitting
        {
            get { return true; }
        }

        public bool IsDefaultFontVariablePitch
        {
            get { return true; }
        }

        public bool SupportsColor
        {
            get { return true; }
        }

        public bool SupportsPictureDisplay
        {
            get { return false; }
        }

        public bool SupportsBoldFont
        {
            get { return true; }
        }

        public bool SupportsItalicFont
        {
            get { return true; }
        }

        public bool SupportsFixedWidthFont
        {
            get { return true; }
        }

        public bool SupportsSoundEffects
        {
            get { return true; }
        }

        public bool SupportsTimedKeyboardInput
        {
            get { return false; }
        }

        public bool SupportsUndo
        {
            get { return false; }
        }

        public bool SupportsMouse
        {
            get { return false; }
        }

        public bool SupportsMenus
        {
            get { return false; }
        }
    }
}
