namespace ZDebug.Compiler
{
    internal delegate ushort ZRoutineCode(byte[] memory, ushort[] locals, ushort[] stack, int sp, ZRoutineCall[] calls, int argumentCount);
}
