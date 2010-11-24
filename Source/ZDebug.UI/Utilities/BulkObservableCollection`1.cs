using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ZDebug.UI.Utilities
{
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        private ReadOnlyObservableCollection<T> readOnly;

        private int bulkOperationCount;
        private bool collectionChangedDuringBulkOperation;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (bulkOperationCount == 0)
            {
                base.OnCollectionChanged(e);
            }
            else
            {
                collectionChangedDuringBulkOperation = true;
            }
        }

        public void BeginBulkOperation()
        {
            bulkOperationCount++;
        }

        public void EndBulkOperation()
        {
            if (bulkOperationCount == 0)
            {
                throw new InvalidOperationException("EndBulkOperation called without matching call to BeginBulkOperation");
            }

            bulkOperationCount--;

            if (bulkOperationCount == 0 && collectionChangedDuringBulkOperation)
            {
                this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                collectionChangedDuringBulkOperation = false;
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            if (items != null)
            {
                BeginBulkOperation();
                try
                {
                    foreach (var item in items)
                    {
                        this.Add(item);
                    }
                }
                finally
                {
                    EndBulkOperation();
                }
            }
        }

        public int BinarySearch<TKey>(TKey key, Func<T, TKey> keySelector, Comparison<TKey> compare)
        {
            var low = 0;
            var high = this.Count - 1;

            while (low <= high)
            {
                var mid = low + ((high - low) / 2);
                var comp = compare(keySelector(this[mid]), key);

                if (comp == 0)
                {
                    return mid;
                }
                else if (comp < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return ~low;
        }

        public int BinarySearch<TKey>(TKey key, Func<T, TKey> keySelector, IComparer<TKey> comparer)
        {
            return BinarySearch(key, keySelector, comparer.Compare);
        }

        public int BinarySearch<TKey>(TKey key, Func<T, TKey> keySelector)
        {
            return BinarySearch(key, keySelector, Comparer<TKey>.Default);
        }

        public int BinarySearch(T item, Comparison<T> compare)
        {
            var low = 0;
            var high = this.Count - 1;

            while (low <= high)
            {
                var mid = low + ((high - low) / 2);
                var comp = compare(this[mid], item);

                if (comp == 0)
                {
                    return mid;
                }
                else if (comp < 0)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            return ~low;
        }

        public int BinarySearch(T item, IComparer<T> comparer)
        {
            return BinarySearch(item, comparer.Compare);
        }

        public int BinarySearch(T item)
        {
            return BinarySearch(item, Comparer<T>.Default);
        }

        public ReadOnlyObservableCollection<T> AsReadOnly()
        {
            if (readOnly == null)
            {
                readOnly = new ReadOnlyObservableCollection<T>(this);
            }

            return readOnly;
        }
    }
}
