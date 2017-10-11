namespace ZDebug.Core.Dictionary
{
    public sealed class ZDictionaryEntry
    {
        private readonly int address;
        private readonly int index;
        private readonly ushort[] zwords;
        private readonly string ztext;
        private readonly byte[] data;

        internal ZDictionaryEntry(int address, int index, ushort[] zwords, string ztext, byte[] data)
        {
            this.address = address;
            this.index = index;
            this.zwords = zwords;
            this.ztext = ztext;
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

        public string ZText
        {
            get { return ztext; }
        }

        public byte[] Data
        {
            get { return data; }
        }
    }
}
