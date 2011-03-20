
namespace ZDebug.Compiler.Generate
{
    public interface IRefLocal : ILocal
    {
        void LoadIndirectValue();
        void StoreIndirectValue();
    }
}
