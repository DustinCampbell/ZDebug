using ZDebug.Core.Execution;

namespace ZDebug.Core.Instructions
{
    public delegate void OpcodeRoutine(Instruction instruction, IExecutionContext context);
}
