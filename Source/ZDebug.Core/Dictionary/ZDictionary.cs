using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Basics;
using ZDebug.Core.Collections;
using ZDebug.Core.Text;
using ZDebug.Core.Utilities;

namespace ZDebug.Core.Dictionary
{
    public sealed class ZDictionary : IIndexedEnumerable<ZDictionaryEntry>
    {
        private readonly Story story;
        private readonly int address;

        private readonly ReadOnlyCollection<char> wordSeparators;

        private readonly List<ZDictionaryEntry> entries;

        internal ZDictionary(Story story)
        {
            this.story = story;

            var memory = story.Memory;
            this.address = memory.ReadDictionaryAddress();

            var reader = memory.CreateReader(address);

            int wordSepCount = reader.NextByte();
            this.wordSeparators = reader.NextBytes(wordSepCount).ConvertAll(b => (char)b).AsReadOnly();

            int entryLength = reader.NextByte();
            int entryCount = reader.NextWord();

            int zwordsSize = story.Version <= 3 ? 2 : 3;
            int dataSize = entryLength - (zwordsSize * 2);

            this.entries = new List<ZDictionaryEntry>(entryCount);
            for (int i = 0; i < entryCount; i++)
            {
                var entryAddress = reader.Address;
                var entryZWords = reader.NextWords(zwordsSize);
                var entryData = reader.NextBytes(dataSize);
                var entryZText = ZText.ZWordsAsString(entryZWords, ZTextFlags.All, memory);
                entries.Add(new ZDictionaryEntry(entryAddress, i, entryZWords, entryZText, entryData));
            }
        }

        public bool TryLookupWord(string word, out ZDictionaryEntry entry)
        {
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var e = entries[i];
                if (word.StartsWith(e.ZText))
                {
                    entry = e;
                    return true;
                }
            }

            entry = default(ZDictionaryEntry);
            return false;
        }

        public ReadOnlyCollection<char> WordSeparators
        {
            get { return wordSeparators; }
        }

        public ZDictionaryEntry this[int index]
        {
            get { return entries[index]; }
        }

        public int Count
        {
            get { return entries.Count; }
        }

        public IEnumerator<ZDictionaryEntry> GetEnumerator()
        {
            foreach (var entry in entries)
            {
                yield return entry;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
