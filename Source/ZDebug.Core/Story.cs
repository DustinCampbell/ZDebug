using System.IO;
using ZDebug.Core.Basics;

namespace ZDebug.Core
{
    public sealed class Story
    {
        private readonly Memory memory;
        private readonly byte version;

        private Story(byte[] bytes)
        {
            this.memory = new Memory(bytes);
            this.version = memory.ReadVersion();
        }

        private Story(Stream stream)
        {
            this.memory = new Memory(stream);
            this.version = memory.ReadVersion();
        }

        public Memory Memory
        {
            get { return memory; }
        }

        public byte Version
        {
            get { return version; }
        }

        public static Story FromBytes(byte[] bytes)
        {
            return new Story(bytes);
        }

        public static Story FromStream(Stream stream)
        {
            return new Story(stream);
        }
    }
}
