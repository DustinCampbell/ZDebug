using System;

namespace ZDebug.Compiler.Generate;

public interface ILocal : IDisposable
{
    void Load();
    void LoadAddress();
    void LoadAndBox();
    void Store();

    void Release();

    int Index { get; }
    Type Type { get; }
}
