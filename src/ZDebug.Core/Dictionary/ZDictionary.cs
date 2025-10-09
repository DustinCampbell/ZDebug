using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ZDebug.Core.Basics;
using ZDebug.Core.Extensions;
using ZDebug.Core.Text;

namespace ZDebug.Core.Dictionary;

public sealed class ZDictionary : IReadOnlyList<ZDictionaryEntry>
{
    private readonly Story story;
    private readonly ZText ztext;
    private readonly int address;

    private readonly ReadOnlyCollection<char> wordSeparators;

    private readonly List<ZDictionaryEntry> entries;

    internal ZDictionary(Story story, ZText ztext)
    {
        this.story = story;
        this.ztext = ztext;

        this.address = Header.ReadDictionaryAddress(story.Memory);

        var reader = new MemoryReader(story.Memory, address);

        int wordSepCount = reader.ReadByte();

        var wordSeps = new byte[wordSepCount];
        reader.CopyBytes(wordSeps);

        this.wordSeparators = wordSeps.ConvertAll(b => (char)b).AsReadOnly();

        int entryLength = reader.ReadByte();
        int entryCount = reader.ReadWord();

        int zwordsSize = story.Version <= 3 ? 2 : 3;
        int dataSize = entryLength - (zwordsSize * 2);

        this.entries = new List<ZDictionaryEntry>(entryCount);
        for (int i = 0; i < entryCount; i++)
        {
            var entryAddress = reader.Address;

            var entryZWords = new ushort[zwordsSize];
            reader.CopyWords(entryZWords);

            var entryData = new byte[dataSize];
            reader.CopyBytes(entryData);

            var entryZText = ztext.ZWordsAsString(entryZWords, ZTextFlags.All);
            entries.Add(new ZDictionaryEntry(entryAddress, i, entryZWords, entryZText, entryData));
        }
    }

    public bool TryLookupWord(string word, out ushort address)
    {
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            var e = entries[i];
            if (word.StartsWith(e.ZText))
            {
                address = (ushort)e.Address;
                return true;
            }
        }

        address = 0;
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
