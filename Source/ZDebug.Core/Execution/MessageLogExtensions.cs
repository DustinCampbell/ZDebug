using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution
{
    internal static class MessageLogExtensions
    {
        public static void SendWarning(this IMessageLog messageLog, string format, params object[] args)
        {
            messageLog.SendWarning(string.Format(format, args));
        }

        public static void SendWarning(this IMessageLog messageLog, Instruction instruction, string message)
        {
            messageLog.SendWarning("{0}: {1} (PC = {2:x4})", instruction.Opcode.Name, message, instruction.Address);
        }

        public static void SendWarning(this IMessageLog messageLog, Instruction instruction, string format, params object[] args)
        {
            messageLog.SendWarning(instruction, string.Format(format, args));
        }

        public static void SendError(this IMessageLog messageLog, string format, params object[] args)
        {
            messageLog.SendError(string.Format(format, args));
        }

        public static void SendError(this IMessageLog messageLog, Instruction instruction, string message)
        {
            messageLog.SendError("{0}: {1} (PC = {2:x4})", instruction.Opcode.Name, message, instruction.Address);
        }

        public static void SendError(this IMessageLog messageLog, Instruction instruction, string format, params object[] args)
        {
            messageLog.SendError(instruction, string.Format(format, args));
        }
    }
}
