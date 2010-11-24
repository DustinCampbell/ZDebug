using System.IO;
using ZDebug.Core.Basics;

namespace ZDebug.Core
{
    public sealed class Story
    {
        private readonly Memory memory;

        private Story(byte[] bytes)
        {
            this.memory = new Memory(bytes);
        }

        private Story(Stream stream)
        {
            this.memory = new Memory(stream);
        }

        public Memory Memory
        {
            get { return memory; }
        }

        public byte Version
        {
            get { return memory.ReadVersion(); }
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
