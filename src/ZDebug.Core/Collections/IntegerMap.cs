using System;
using System.Collections.Generic;

namespace ZDebug.Core.Collections;

public class IntegerMap<T>
{
    private struct Entry
    {
        public int Key;
        public T Value;
        public int Next;
    }

    private Entry[] entries;
    private int[] buckets;
    private int freeList;
    private int freeCount;
    private int count;

    public IntegerMap()
    {
        Initialize(463);
    }

    public IntegerMap(int capacity)
    {
        Initialize(capacity);
    }

    private void Initialize(int capacity)
    {
        var prime = HashHelpers.GetPrime(capacity);
        var buckets = new int[prime];
        for (var i = 0; i < buckets.Length; i++)
        {
            buckets[i] = -1;
        }
        this.buckets = buckets;
        entries = new Entry[prime];
        freeList = -1;
    }

    private void Insert(int key, T value)
    {
        var index = key % buckets.Length;

        var entries = this.entries;

        for (var i = buckets[index]; i >= 0; i = entries[i].Next)
        {
            if (entries[i].Key == key)
            {
                throw new ArgumentException("Cannot not add duplicate key.", "key");
            }
        }

        int freeEntry;
        if (freeCount > 0)
        {
            freeEntry = freeList;
            freeList = entries[freeEntry].Next;
            freeCount--;
        }
        else
        {
            if (count == this.entries.Length)
            {
                Resize();
                entries = this.entries;
                index = key % buckets.Length;
            }

            freeEntry = count;
            count++;
        }

        entries[freeEntry].Next = buckets[index];
        entries[freeEntry].Key = key;
        entries[freeEntry].Value = value;
        buckets[index] = freeEntry;
    }

    private void Resize()
    {
        var prime = HashHelpers.GetPrime(count * 2);

        var newBuckets = new int[prime];
        for (var i = 0; i < prime; i++)
        {
            newBuckets[i] = -1;
        }

        var newEntries = new Entry[prime];
        var oldCount = count;
        Array.Copy(entries, 0, newEntries, 0, oldCount);
        for (var i = 0; i < oldCount; i++)
        {
            var index = newEntries[i].Key % prime;
            newEntries[i].Next = newBuckets[index];
            newBuckets[index] = i;
        }

        buckets = newBuckets;
        entries = newEntries;
    }

    public void Add(int key, T value) => Insert(key, value);

    public void Clear()
    {
        if (count > 0)
        {
            var buckets = this.buckets;
            for (var i = 0; i < buckets.Length; i++)
            {
                buckets[i] = -1;
            }

            Array.Clear(entries, 0, count);
            freeList = -1;
            count = 0;
            freeCount = 0;
        }
    }

    public bool Contains(int key) => FindEntry(key) >= 0;

    private int FindEntry(int key)
    {
        var index = key % buckets.Length;

        var entries = this.entries;
        for (var i = buckets[index]; i >= 0; i = entries[i].Next)
        {
            if (entries[i].Key == key)
            {
                return i;
            }
        }

        return -1;
    }

    public bool TryGetValue(int key, out T value)
    {
        var index = FindEntry(key);
        if (index >= 0)
        {
            value = entries[index].Value;
            return true;
        }

        value = default(T);
        return false;
    }

    public T this[int key]
    {
        get
        {
            var index = FindEntry(key);
            if (index >= 0)
            {
                return entries[index].Value;
            }

            throw new KeyNotFoundException();
        }
    }

    public int Count => count;
}
