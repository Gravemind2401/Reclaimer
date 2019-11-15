using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Models
{
    public abstract class TabWellBase : TabOwnerBase
    {
        public ObservableCollection<TabItem> Children { get; }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set { SetProperty(ref isActive, value); }
        }

        private TabItem selectedItem;
        public TabItem SelectedItem
        {
            get { return selectedItem; }
            set { SetProperty(ref selectedItem, value); }
        }

        private double width;
        public double Width
        {
            get { return width; }
            set { SetProperty(ref width, value, UpdateChildrenWidth); }
        }

        private double height;
        public double Height
        {
            get { return height; }
            set { SetProperty(ref height, value, UpdateChildrenHeight); }
        }

        public DelegateCommand<TabItem> CloseTabCommand { get; }
        public DelegateCommand<TabItem> TogglePinStatusCommand { get; }
        public DelegateCommand<TabItem> SelectItemCommand { get; }
        //public DelegateCommand<FloatEventArgs> FloatTabCommand { get; }
        //public DelegateCommand<FloatEventArgs> FloatAllCommand { get; }
        //public DelegateCommand<DockEventArgs> DockCommand { get; }

        public TabWellBase()
        {
            CloseTabCommand = new DelegateCommand<TabItem>(CloseTabExecuted);
            TogglePinStatusCommand = new DelegateCommand<TabItem>(TogglePinStatusExecuted);
            SelectItemCommand = new DelegateCommand<TabItem>(SelectItemExecuted);
            //FloatTabCommand = new DelegateCommand<FloatEventArgs>(FloatTabExecuted);
            //FloatAllCommand = new DelegateCommand<FloatEventArgs>(FloatAllExecuted);
            //DockCommand = new DelegateCommand<DockEventArgs>(DockExecuted);

            Children = new ObservableCollection<TabItem>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        protected virtual void CloseTabExecuted(TabItem item)
        {
            var parent = ParentContainer;
            Children.Remove(item ?? SelectedItem);

            if (parent != null && parent.IsRafted && !parent.AllTabs.Any())
                parent.Host.Close();
        }

        protected virtual void TogglePinStatusExecuted(TabItem item)
        {

        }

        protected virtual void SelectItemExecuted(TabItem item)
        {
            SelectedItem = item;
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var tab in e.OldItems.OfType<TabItem>())
                    tab.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var tab in e.NewItems.OfType<TabItem>())
                    tab.Parent = this;
            }

            OnChildrenChanged();
        }

        protected virtual void OnChildrenChanged()
        {

        }

        private void UpdateChildrenWidth()
        {
            foreach (var item in Children)
                item.Width = Width;
        }

        private void UpdateChildrenHeight()
        {
            foreach (var item in Children)
                item.Height = Height;
        }

        internal override IEnumerable<TabItem> AllTabs => Children;
    }
}
