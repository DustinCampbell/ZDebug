
using System;

namespace ZDebug.Core.Instructions;

public sealed class Variable
{
    private const byte LocalsOffset = 1;
    private const byte LocalsLength = 15;
    private const byte GlobalsOffset = 16;
    private const byte GlobalsLength = 240;

    private static readonly Variable[] s_variables = InitializeVariable();

    public static Variable Stack => s_variables[0];
    public static ReadOnlySpan<Variable> Locals => s_variables.AsSpan(LocalsOffset, LocalsLength);
    public static ReadOnlySpan<Variable> Globals => s_variables.AsSpan(GlobalsOffset, GlobalsLength);

    private static Variable[] InitializeVariable()
    {
        var variables = new Variable[256];

        variables[0] = new(VariableKind.Stack, 0);

        var locals = variables.AsSpan(LocalsOffset, LocalsLength);

        for (byte i = 0; i < LocalsLength; i++)
        {
            locals[i] = new(VariableKind.Local, i);
        }

        var globals = variables.AsSpan(GlobalsOffset, GlobalsLength);

        for (byte i = 0; i < GlobalsLength; i++)
        {
            globals[i] = new(VariableKind.Global, i);
        }

        return variables;
    }

    public readonly byte ByteValue;
    public readonly VariableKind Kind;
    public readonly byte Index;
    public readonly string DisplayText;

    private Variable(VariableKind kind, byte index)
    {
        Kind = kind;
        Index = index;

        (ByteValue, DisplayText) = kind switch
        {
            VariableKind.Stack => ((byte)0, "SP"),
            VariableKind.Local => ((byte)(LocalsOffset + index), $"L{index:x2}"),

            // VariableKind.Global
            _ => ((byte)(GlobalsOffset + index), $"G{index:x2}")
        };
    }

    public override string ToString()
        => DisplayText;

    public static Variable FromByte(byte b)
        => s_variables[b];
}
