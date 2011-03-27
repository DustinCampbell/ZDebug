using System;
using System.Collections.Generic;

namespace ZDebug.Core.Collections
{
    public class IntegerMap<T>
    {
        private const int DEFAULT_BUCKET_SIZE = 4;

        private struct Entry
        {
            public readonly int Key;
            public readonly T Value;

            public Entry(int key, T value)
            {
                this.Key = key;
                this.Value = value;
            }
        }

        private struct Bucket
        {
            public readonly Entry[] Entries;
            public readonly int Size;

            public Bucket(Entry[] entries, int size)
            {
                this.Entries = entries;
                this.Size = size;
            }
        }

        private Bucket[] buckets;
        private int capacity;
        private int size;
        private int loadSize;

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
            int prime = HashHelpers.GetPrime(capacity);
            this.capacity = prime;
            this.buckets = new Bucket[this.capacity];
            this.loadSize = (int)(this.capacity * 0.7);
        }

        private void Insert(int key, T value)
        {
            int hash = key % buckets.Length;

            var bucket = buckets[hash];
            var bucketSize = bucket.Size;
            if (bucketSize == 0)
            {
                var entries = new Entry[DEFAULT_BUCKET_SIZE];
                entries[0] = new Entry(key, value);
                bucket = new Bucket(entries, 1);
                buckets[hash] = bucket;
            }
            else
            {
                var entries = bucket.Entries;

                for (int i = 0; i < bucketSize; i++)
                {
                    if (entries[i].Key == key)
                    {
                        throw new ArgumentException("Attempted to add duplicate key: " + key);
                    }
                }

                if (entries.Length == bucketSize)
                {
                    var newEntries = new Entry[entries.Length * 2];
                    Array.Copy(entries, 0, newEntries, 0, entries.Length);
                    entries = newEntries;
                }

                entries[bucketSize] = new Entry(key, value);
                var newBucket = new Bucket(entries, bucketSize + 1);
                buckets[hash] = newBucket;
            }
            size++;
        }

        private void Resize()
        {
            var newMap = new IntegerMap<T>(buckets.Length * 2);

            for (int i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i];
                var bucketSize = bucket.Size;
                if (bucketSize > 0)
                {
                    var entries = bucket.Entries;
                    for (int j = 0; j < bucketSize; j++)
                    {
                        var entry = entries[j];
                        newMap.Insert(entry.Key, entry.Value);
                    }
                }
            }

            this.buckets = newMap.buckets;
            this.capacity = newMap.capacity;
            this.size = newMap.size;
            this.loadSize = newMap.loadSize;
        }

        public void Add(int key, T value)
        {
            if (size == loadSize)
            {
                Resize();
            }

            Insert(key, value);
        }

        public void Clear()
        {
            this.buckets = new Bucket[this.capacity];
            size = 0;
        }

        public bool Contains(int key)
        {
            int hash = key % capacity;

            var bucket = buckets[hash];
            var bucketSize = bucket.Size;
            if (bucketSize > 0)
            {
                var entries = bucket.Entries;
                for (int i = 0; i < bucketSize; i++)
                {
                    if (entries[i].Key == key)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryGetValue(int key, out T value)
        {
            int hash = key % capacity;

            var bucket = buckets[hash];
            var bucketSize = bucket.Size;
            if (bucketSize > 0)
            {
                var entries = bucket.Entries;
                for (int i = 0; i < bucketSize; i++)
                {
                    var entry = entries[i];
                    if (entry.Key == key)
                    {
                        value = entry.Value;
                        return true;
                    }
                }
            }

            value = default(T);
            return false;
        }

        public T this[int key]
        {
            get
            {
                int hash = key % capacity;

                var bucket = buckets[hash];
                if (bucket.Entries != null)
                {
                    var entries = bucket.Entries;
                    var bucketSize = bucket.Size;
                    for (int i = 0; i < bucketSize; i++)
                    {
                        var entry = entries[i];
                        if (entry.Key == key)
                        {
                            return entry.Value;
                        }
                    }
                }

                throw new KeyNotFoundException();
            }
        }

        public int Count
        {
            get { return size; }
        }
    }
}
