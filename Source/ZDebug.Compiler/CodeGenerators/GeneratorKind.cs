namespace ZDebug.Compiler.CodeGenerators
{
    internal enum GeneratorKind
    {
        LoadInteger,
        LoadLocal,
        LoadRefLocalValue,
        DecrementRefLocal,
        IncrementRefLocal,
        MemoryLoadByte,
        MemoryLoadWord,
        MemoryStoreByte,
        MemoryStoreWord,
        StackEmptyTest,
        StackFullTest,
        StackPeek,
        StackPop,
        StackSetTop,
        StackPush,
        LoadLocalVariable,
        StoreLocalVariable,
    }
}
