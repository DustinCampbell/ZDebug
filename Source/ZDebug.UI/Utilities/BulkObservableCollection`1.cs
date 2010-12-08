using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ZDebug.UI.Utilities
{
    public class BulkObservableCollection<T> : Collection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private int bulkOperationCount;
        private bool collectionChangedDuringBulkOperation;

        protected void OnPropertyChanged(string name)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, T item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, T item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, T item, T oldItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, oldItem, index));
        }

        private void OnCollectionReset()
        {
            OnPropertyChanged("Count");
            OnPropertyChanged("Items[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            var handler = CollectionChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            if (bulkOperationCount == 0)
            {
                OnCollectionReset();
            }
            else
            {
                collectionChangedDuringBulkOperation = true;
            }
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (bulkOperationCount == 0)
            {
                OnPropertyChanged("Count");
                OnPropertyChanged("Items[]");
                OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
            }
            else
            {
                collectionChangedDuringBulkOperation = true;
            }
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);

            if (bulkOperationCount == 0)
            {
                OnPropertyChanged("Count");
                OnPropertyChanged("Items[]");
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
            }
            else
            {
                collectionChangedDuringBulkOperation = true;
            }
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];

            base.SetItem(index, item);

            if (bulkOperationCount == 0)
            {
                OnPropertyChanged("Items[]");
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, item, oldItem, index);
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
                OnCollectionReset();
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
                    var list = items as IList<T>;
                    if (list != null)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            this.Add(list[i]);
                        }
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            this.Add(item);
                        }
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

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
