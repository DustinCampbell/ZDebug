using ZDebug.Core.Execution;

namespace ZDebug.Core.Instructions
{
    public abstract class OpcodeRoutine
    {
        public abstract void Invoke(Instruction i, ushort[] opValues, int opCount, IExecutionContext context);
    }
}
