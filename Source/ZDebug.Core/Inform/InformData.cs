using ZDebug.Core.Basics;

namespace ZDebug.Core.Inform
{
    public sealed class InformData
    {
        private readonly Memory memory;
        private readonly int version;

        public InformData(Memory memory)
        {
            this.memory = memory;
            this.version = memory.ReadInformVersionNumber();
        }

        public int Version
        {
            get { return version; }
        }
    }
}
