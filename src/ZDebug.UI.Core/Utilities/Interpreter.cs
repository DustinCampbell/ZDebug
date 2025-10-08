using ZDebug.Core.Interpreter;

namespace ZDebug.UI.Utilities;

public sealed class Interpreter : IInterpreter
{
    public InterpreterTarget Target => InterpreterTarget.IBMPC;

    public byte Version => (byte)'A';

    public byte StandardRevisionMajorVersion => 1;

    public byte StandardRevisionMinorVersion => 0;

    public bool SupportsStatusLine => true;

    public bool SupportsScreenSplitting => true;

    public bool IsDefaultFontVariablePitch => true;

    public bool SupportsColor => true;

    public bool SupportsPictureDisplay => false;

    public bool SupportsBoldFont => true;

    public bool SupportsItalicFont => true;

    public bool SupportsFixedWidthFont => true;

    public bool SupportsSoundEffects => true;

    public bool SupportsTimedKeyboardInput => false;

    public bool SupportsUndo => false;

    public bool SupportsMouse => false;

    public bool SupportsMenus => false;
}
