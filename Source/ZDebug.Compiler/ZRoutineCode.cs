namespace ZDebug.Compiler
{
    internal delegate ushort ZRoutineCode(ushort[] locals, ushort[] stack, int sp, ZRoutineCall[] calls, int argumentCount);
}
