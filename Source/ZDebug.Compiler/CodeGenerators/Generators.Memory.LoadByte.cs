using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Memory
        {
            internal class LoadByte : Generator
            {
                private readonly IArrayLocal memory;
                private readonly Generator loadIndex;

                public LoadByte(ILBuilder il, IArrayLocal memory, Generator loadIndex)
                    : base(il, GeneratorKind.MemoryLoadByte)
                {
                    this.memory = memory;
                    this.loadIndex = loadIndex;
                }

                public override void Generate()
                {
                    memory.LoadElement(
                        indexLoader: () =>
                        {
                            loadIndex.Generate();
                        });
                }
            }
        }
    }
}
