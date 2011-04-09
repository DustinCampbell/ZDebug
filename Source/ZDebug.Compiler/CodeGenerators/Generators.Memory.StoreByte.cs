using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Memory
        {
            internal class StoreByte : Generator
            {
                private readonly IArrayLocal memory;
                private readonly Generator loadIndex;
                private readonly Generator loadValue;

                public StoreByte(ILBuilder il, IArrayLocal memory, Generator loadIndex, Generator loadValue)
                    : base(il, GeneratorKind.MemoryStoreByte)
                {
                    this.memory = memory;
                    this.loadIndex = loadIndex;
                    this.loadValue = loadValue;
                }

                public override void Generate()
                {
                    memory.StoreElement(
                        indexLoader: () =>
                        {
                            loadIndex.Generate();
                        },
                        valueLoader: () =>
                        {
                            loadValue.Generate();
                        });
                }
            }
        }
    }
}
