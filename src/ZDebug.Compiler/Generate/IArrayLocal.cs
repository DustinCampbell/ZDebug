namespace ZDebug.Compiler.Generate
{
    public interface IArrayLocal : ILocal
    {
        void Create(int length);
        void Create(ILocal length);

        void LoadLength();

        void LoadElement(CodeBuilder indexLoader);
        void StoreElement(CodeBuilder indexLoader, CodeBuilder valueLoader);
    }
}
