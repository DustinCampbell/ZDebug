using System.Collections.ObjectModel;
using ZDebug.Core.Extensions;
using ZDebug.Core.Instructions;

namespace ZDebug.Core.Execution;

public sealed class StackFrame
{
    private readonly uint callAddress;
    private readonly ReadOnlyCollection<ushort> arguments;
    private readonly ReadOnlyCollection<ushort> locals;
    private readonly uint returnAddress;
    private readonly Variable storeVariable;

    internal StackFrame(uint callAddress, ushort[] arguments, ushort[] locals, uint returnAddress, Variable storeVariable)
    {
        this.callAddress = callAddress;
        this.arguments = arguments.AsReadOnly();
        this.locals = locals.AsReadOnly();
        this.returnAddress = returnAddress;
        this.storeVariable = storeVariable;
    }

    public uint CallAddress => callAddress;

    public ReadOnlyCollection<ushort> Arguments => arguments;

    public ReadOnlyCollection<ushort> Locals => locals;

    public uint ReturnAddress => returnAddress;

    public Variable StoreVariable => storeVariable;
}
