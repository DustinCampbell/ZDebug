namespace ZDebug.Core.Interpreter
{
    /// <summary>
    /// Core interface to be implemented by the Z-machine interpreter.
    /// Call <see cref="Story.RegisterInterpreter"/> to register a Z-machine
    /// interpreter with a story.
    /// </summary>
    public interface IInterpreter
    {
        /// <summary>
        /// Gets the target machine of this interpreter.
        /// </summary>
        InterpreterTarget Target { get; }

        /// <summary>
        /// Gets the version number of this interpreter.
        /// </summary>
        byte Version { get; }

        /// <summary>
        /// Gets the major portion of the standard revision version number supported
        /// by this interpreter.
        /// </summary>
        byte StandardRevisionMajorVersion { get; }

        /// <summary>
        /// Gets the minor portion of the standard revision version number supported
        /// by this interpreter.
        /// </summary>
        byte StandardRevisionMinorVersion { get; }

        /// <summary>
        /// Returns true if this interpreter supports drawing a status line for V1-V3 stories; otherwise, false.
        /// </summary>
        bool SupportsStatusLine { get; }

        /// <summary>
        /// Returns true if this interpreter supports screen-splitting for V3 stories; otherwise, false.
        /// </summary>
        bool SupportsScreenSplitting { get; }

        /// <summary>
        /// Returns true if this interpreter's default font is variable-pitch for V3 stories; otherwise, false.
        /// </summary>
        bool IsDefaultFontVariablePitch { get; }

        /// <summary>
        /// Returns true if this interpreter supports color for V5+ stories; otherwise, false.
        /// </summary>
        bool SupportsColor { get; }

        /// <summary>
        /// Returns true if this interpreter supports picture display for V6 stories; otherwise, false.
        /// </summary>
        bool SupportsPictureDisplay { get; }

        /// <summary>
        /// Returns true if this interpreter supports bold fonts for V4+ stories; otherwise, false.
        /// </summary>
        bool SupportsBoldFont { get; }

        /// <summary>
        /// Returns true if this interpreter supports italic fonts for V4+ stories; otherwise, false.
        /// </summary>
        bool SupportsItalicFont { get; }

        /// <summary>
        /// Returns true if this interpreter supports fixed-width fonts for V4+ stories; otherwise, false.
        /// </summary>
        bool SupportsFixedWidthFont { get; }

        /// <summary>
        /// Returns true if this interpreter supports sound effects for V6 stories; otherwise, false.
        /// </summary>
        bool SupportsSoundEffects { get; }

        /// <summary>
        /// Returns true if this interpreter supports timed keyboard input for V4+ stories; otherwise, false.
        /// </summary>
        bool SupportsTimedKeyboardInput { get; }

        /// <summary>
        /// Returns true if this interpreter supports undo for V5+ stories; otherwise, false.
        /// </summary>
        bool SupportsUndo { get; }

        /// <summary>
        /// Returns true if this interpreter supports the mouse for V5+ stories; otherwise, false.
        /// </summary>
        bool SupportsMouse { get; }

        /// <summary>
        /// Returns true if this interpreter supports the menus for V6 stories; otherwise, false.
        /// </summary>
        bool SupportsMenus { get; }
    }
}
