namespace ZDebug.Core.Interpreter;

internal sealed class DefaultInterpreter : IInterpreter
{
    public InterpreterTarget Target => InterpreterTarget.IBMPC;

    public byte Version => (byte)'A';

    public byte StandardRevisionMajorVersion => 1;

    public byte StandardRevisionMinorVersion => 0;

    public bool SupportsStatusLine => false;

    public bool SupportsScreenSplitting => false;

    public bool IsDefaultFontVariablePitch => false;

    public bool SupportsColor => false;

    public bool SupportsPictureDisplay => false;

    public bool SupportsBoldFont => false;

    public bool SupportsItalicFont => false;

    public bool SupportsFixedWidthFont => false;

    public bool SupportsSoundEffects => false;

    public bool SupportsTimedKeyboardInput => false;

    public bool SupportsUndo => false;

    public bool SupportsMouse => false;

    public bool SupportsMenus => false;
}
