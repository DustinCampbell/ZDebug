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
    }
}
