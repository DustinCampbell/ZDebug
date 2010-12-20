using ZDebug.Core.Execution;

namespace ZDebug.Core.Instructions
{
    public delegate void OpcodeRoutine(Instruction instruction, ushort[] operandValues, int operandCount, IExecutionContext context);
}
