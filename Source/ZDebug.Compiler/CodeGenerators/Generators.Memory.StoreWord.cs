using ZDebug.Compiler.Generate;

namespace ZDebug.Compiler.CodeGenerators
{
    internal partial class Generators
    {
        internal partial class Memory
        {
            internal class StoreWord : Generator
            {
                private readonly IArrayLocal memory;
                private readonly Generator loadIndex;
                private readonly Generator loadIndexPlusOne;
                private readonly Generator loadValue;

                public StoreWord(ILBuilder il, IArrayLocal memory, Generator loadIndex, Generator loadIndexPlusOne, Generator loadValue)
                    : base(il, GeneratorKind.MemoryStoreWord)
                {
                    this.memory = memory;
                    this.loadIndex = loadIndex;
                    this.loadIndexPlusOne = loadIndexPlusOne;
                    this.loadValue = loadValue;
                }

                public override void Generate()
                {
                    // memory[address] = (byte)(value >> 8);
                    memory.StoreElement(
                        indexLoader: () =>
                        {
                            loadIndex.Generate();
                        },
                        valueLoader: () =>
                        {
                            loadValue.Generate();
                            IL.Math.Shr(8);
                            IL.Convert.ToUInt8();
                        });

                    // memory[address + 1] = (byte)(value & 0xff);
                    memory.StoreElement(
                        indexLoader: () =>
                        {
                            loadIndexPlusOne.Generate();
                        },
                        valueLoader: () =>
                        {
                            loadValue.Generate();
                            IL.Math.And(0xff);
                            IL.Convert.ToUInt8();
                        });
                }
            }
        }
    }
}
