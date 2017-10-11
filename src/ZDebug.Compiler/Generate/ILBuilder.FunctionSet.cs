namespace ZDebug.Compiler.Generate
{
    public sealed partial class ILBuilder
    {
        public abstract class FunctionSet
        {
            protected readonly ILBuilder builder;

            protected FunctionSet(ILBuilder builder)
            {
                this.builder = builder;
            }
        }
    }
}
