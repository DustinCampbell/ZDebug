using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZDebug.Core.Interpreter
{
    internal sealed class DefaultInterpreter : IInterpreter
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
            get { return false; }
        }

        public bool SupportsScreenSplitting
        {
            get { return false; }
        }

        public bool IsDefaultFontVariablePitch
        {
            get { return false; }
        }

        public bool SupportsColor
        {
            get { return false; }
        }

        public bool SupportsPictureDisplay
        {
            get { return false; }
        }

        public bool SupportsBoldFont
        {
            get { return false; }
        }

        public bool SupportsItalicFont
        {
            get { return false; }
        }

        public bool SupportsFixedWidthFont
        {
            get { return false; }
        }

        public bool SupportsSoundEffects
        {
            get { return false; }
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
