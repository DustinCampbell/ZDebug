namespace ZDebug.Core.Instructions;

public sealed class Opcode
{
    public readonly OpcodeKind Kind;
    public readonly byte Number;
    public readonly string Name;
    public readonly bool HasStoreVariable;
    public readonly bool HasBranch;
    public readonly bool HasZText;
    public readonly bool IsCall;
    public readonly bool IsDoubleVariable;
    public readonly bool IsFirstOpByRef;
    public readonly bool IsJump;
    public readonly bool IsQuit;
    public readonly bool IsReturn;

    internal Opcode(OpcodeKind kind, byte number, string name, OpcodeFlags flags)
    {
        Kind = kind;
        Number = number;
        Name = name;
        HasStoreVariable = (flags & OpcodeFlags.Store) != 0;
        HasBranch = (flags & OpcodeFlags.Branch) != 0;
        HasZText = (flags & OpcodeFlags.ZText) != 0;
        IsCall = (flags & OpcodeFlags.Call) != 0;
        IsDoubleVariable = (flags & OpcodeFlags.DoubleVar) != 0;
        IsFirstOpByRef = (flags & OpcodeFlags.FirstOpByRef) != 0;
        IsJump = kind == OpcodeKind.OneOp && number == 0x0c;
        IsQuit = kind == OpcodeKind.ZeroOp && number == 0x0a;
        IsReturn = (flags & OpcodeFlags.Return) != 0;
    }

    public override string ToString() => string.Format("{{{0} ({1} - {2})}}", Name, Kind, Number);
}
