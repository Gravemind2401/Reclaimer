using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utilities
{
    public class DeepObservableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public event EventHandler<ChildPropertyChangedEventArgs> ChildPropertyChanged;

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            item.PropertyChanged += Item_PropertyChanged;
        }

        protected override void SetItem(int index, T item)
        {
            Items[index].PropertyChanged -= Item_PropertyChanged;
            base.SetItem(index, item);
            item.PropertyChanged += Item_PropertyChanged;
        }

        protected override void RemoveItem(int index)
        {
            Items[index].PropertyChanged -= Item_PropertyChanged;
            base.RemoveItem(index);
        }

        protected override void ClearItems()
        {
            foreach (var item in Items)
                item.PropertyChanged -= Item_PropertyChanged;

            base.ClearItems();
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e) => ChildPropertyChanged?.Invoke(this, new ChildPropertyChangedEventArgs(sender, e.PropertyName));
    }

    public class ChildPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public virtual object Element { get; }

        public ChildPropertyChangedEventArgs(object element, string propertyName)
            : base(propertyName)
        {
            Element = element;
        }
    }
}
