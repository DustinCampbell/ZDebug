using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Memory
        {
            internal class LoadWord : Generator
            {
                private readonly IArrayLocal memory;
                private readonly Generator loadIndex;
                private readonly Generator loadIndexPlusOne;

                public LoadWord(ILBuilder il, IArrayLocal memory, Generator loadIndex, Generator loadIndexPlusOne)
                    : base(il, GeneratorKind.MemoryLoadWord)
                {
                    this.memory = memory;
                    this.loadIndex = loadIndex;
                }

                public override void Generate()
                {
                    // shift memory[address] left 8 bits
                    memory.LoadElement(
                        indexLoader: () =>
                        {
                            loadIndex.Generate();
                        });
                    IL.Math.Shl(8);

                    // read memory[address + 1]
                    memory.LoadElement(
                        indexLoader: () =>
                        {
                            loadIndexPlusOne.Generate();
                        });

                    // or bytes together
                    IL.Math.Or();
                    IL.Convert.ToUInt16();
                }
            }
        }
    }
}
