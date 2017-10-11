using System;
using System.Collections.Generic;

namespace ZDebug.Core.Collections
{
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
            int prime = HashHelpers.GetPrime(capacity);
            var buckets = new int[prime];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = -1;
            }
            this.buckets = buckets;
            this.entries = new Entry[prime];
            this.freeList = -1;
        }

        private void Insert(int key, T value)
        {
            int index = key % this.buckets.Length;

            var entries = this.entries;

            for (int i = this.buckets[index]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].Key == key)
                {
                    throw new ArgumentException("Cannot not add duplicate key.", "key");
                }
            }

            int freeEntry;
            if (this.freeCount > 0)
            {
                freeEntry = this.freeList;
                this.freeList = entries[freeEntry].Next;
                this.freeCount--;
            }
            else
            {
                if (this.count == this.entries.Length)
                {
                    this.Resize();
                    entries = this.entries;
                    index = key % this.buckets.Length;
                }

                freeEntry = this.count;
                this.count++;
            }

            entries[freeEntry].Next = this.buckets[index];
            entries[freeEntry].Key = key;
            entries[freeEntry].Value = value;
            this.buckets[index] = freeEntry;
        }

        private void Resize()
        {
            int prime = HashHelpers.GetPrime(this.count * 2);

            var newBuckets = new int[prime];
            for (int i = 0; i < prime; i++)
            {
                newBuckets[i] = -1;
            }

            var newEntries = new Entry[prime];
            var oldCount = this.count;
            Array.Copy(this.entries, 0, newEntries, 0, oldCount);
            for (int i = 0; i < oldCount; i++)
            {
                var index = newEntries[i].Key % prime;
                newEntries[i].Next = newBuckets[index];
                newBuckets[index] = i;
            }

            this.buckets = newBuckets;
            this.entries = newEntries;
        }

        public void Add(int key, T value)
        {
            Insert(key, value);
        }

        public void Clear()
        {
            if (this.count > 0)
            {
                var buckets = this.buckets;
                for (int i = 0; i < buckets.Length; i++)
                {
                    buckets[i] = -1;
                }

                Array.Clear(this.entries, 0, this.count);
                this.freeList = -1;
                this.count = 0;
                this.freeCount = 0;
            }
        }

        public bool Contains(int key)
        {
            return FindEntry(key) >= 0;
        }

        private int FindEntry(int key)
        {
            int index = key % this.buckets.Length;

            var entries = this.entries;
            for (int i = this.buckets[index]; i >= 0; i = entries[i].Next)
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
            int index = FindEntry(key);
            if (index >= 0)
            {
                value = this.entries[index].Value;
                return true;
            }

            value = default(T);
            return false;
        }

        public T this[int key]
        {
            get
            {
                int index = FindEntry(key);
                if (index >= 0)
                {
                    return this.entries[index].Value;
                }

                throw new KeyNotFoundException();
            }
        }

        public int Count
        {
            get { return this.count; }
        }
    }
}
