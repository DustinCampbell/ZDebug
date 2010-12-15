namespace ZDebug.Core.Dictionary
{
    public sealed class ZDictionaryEntry
    {
        private readonly int address;
        private readonly int index;
        private readonly ushort[] zwords;
        private readonly byte[] data;

        internal ZDictionaryEntry(int address, int index, ushort[] zwords, byte[] data)
        {
            this.address = address;
            this.index = index;
            this.zwords = zwords;
            this.data = data;
        }

        public int Address
        {
            get { return address; }
        }

        public int Index
        {
            get { return index; }
        }

        public ushort[] ZWords
        {
            get { return zwords; }
        }

        public byte[] Data
        {
            get { return data; }
        }
    }
}
