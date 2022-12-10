using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Reclaimer.Utilities
{
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        public virtual void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items)
                Items.Add(item);

            RaiseCollectionReset(0);
        }

        public virtual void RemoveRange(int index, int count)
        {
            var lastCount = Count;

            for (var i = index + count - 1; i >= index; i--)
                Items.RemoveAt(i);

            RaiseCollectionReset(lastCount);
        }

        public virtual void RemoveAll(Predicate<T> match)
        {
            var lastCount = Count;

            for (var i = Count - 1; i >= 0; i--)
            {
                if (match(Items[i]))
                    Items.RemoveAt(i);
            }

            RaiseCollectionReset(lastCount);
        }

        public virtual void Reset(IEnumerable<T> items)
        {
            var lastCount = Count;

            Items.Clear();
            foreach (var item in items)
                Items.Add(item);

            RaiseCollectionReset(lastCount);
        }

        protected void RaiseCollectionReset(int lastCount)
        {
            if (Count != lastCount)
                RaisePropertyChanged(nameof(Count));

            RaisePropertyChanged("Item[]");
            RaiseCollectionChanged(NotifyCollectionChangedAction.Reset);
        }

        protected void RaisePropertyChanged(string propertyName) => OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        protected void RaiseCollectionChanged(NotifyCollectionChangedAction action) => OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
    }
}
