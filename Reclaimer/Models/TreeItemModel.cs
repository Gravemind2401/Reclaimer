using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Models
{
    public class TreeItemModel : BindableBase
    {
        public ObservableCollection<TreeItemModel> Items { get; }

        public bool HasItems => Items.Count > 0;

        private TreeItemModel parent;
        public TreeItemModel Parent
        {
            get { return parent; }
            private set { SetProperty(ref parent, value); }
        }

        private string header;
        public string Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        private bool? isChecked;
        public bool? IsChecked
        {
            get { return isChecked; }
            set { SetProperty(ref isChecked, value); }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }

        private object tag;
        public object Tag
        {
            get { return tag; }
            set { SetProperty(ref tag, value); }
        }

        public bool IsVisible => Visibility == Visibility.Visible;

        private Visibility visibility;
        public Visibility Visibility
        {
            get { return visibility; }
            set
            {
                if (SetProperty(ref visibility, value))
                    RaisePropertyChanged(nameof(IsVisible));
            }
        }

        public TreeItemModel()
        {
            Items = new ObservableCollection<TreeItemModel>();
            Items.CollectionChanged += Items_CollectionChanged;
        }

        public TreeItemModel(string header) : this()
        {
            Header = header;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<TreeItemModel>())
                    item.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<TreeItemModel>())
                {
                    if (item.Parent != null)
                        throw new InvalidOperationException();

                    item.Parent = this;
                }
            }

            RaisePropertyChanged(nameof(HasItems));
        }

        public override string ToString() => Header ?? base.ToString();
    }
}
