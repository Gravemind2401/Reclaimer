using Studio.Controls;
using System.Collections.ObjectModel;

namespace Reclaimer.Models
{
    public abstract class TabWellModelBase : TabOwnerModelBase
    {
        public ObservableCollection<TabModel> Children { get; }

        public bool IsActive
        {
            get;
            set => SetProperty(ref field, value);
        }

        public TabModel SelectedItem
        {
            get;
            set => SetProperty(ref field, value);
        }

        public double Width
        {
            get;
            set => SetProperty(ref field, value, UpdateChildrenWidth);
        }

        public double Height
        {
            get;
            set => SetProperty(ref field, value, UpdateChildrenHeight);
        }

        public DelegateCommand<TabModel> CloseTabCommand { get; }
        public DelegateCommand<TabModel> TogglePinStatusCommand { get; }
        public DelegateCommand<TabModel> SelectItemCommand { get; }
        public DelegateCommand<FloatEventArgs> FloatTabCommand { get; }
        public DelegateCommand<FloatEventArgs> FloatAllCommand { get; }
        public DelegateCommand<DockEventArgs> DockCommand { get; }

        public TabWellModelBase()
        {
            CloseTabCommand = new DelegateCommand<TabModel>(CloseTabExecuted);
            TogglePinStatusCommand = new DelegateCommand<TabModel>(TogglePinStatusExecuted);
            SelectItemCommand = new DelegateCommand<TabModel>(SelectItemExecuted);
            FloatTabCommand = new DelegateCommand<FloatEventArgs>(FloatTabExecuted);
            FloatAllCommand = new DelegateCommand<FloatEventArgs>(FloatAllExecuted);
            DockCommand = new DelegateCommand<DockEventArgs>(DockExecuted);

            Children = new ObservableCollection<TabModel>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        protected virtual void CloseTabExecuted(TabModel item)
        {
            var parent = ParentContainer;

            item ??= SelectedItem;
            Children.Remove(item);
            item.Dispose();

            if (parent != null && parent.IsRafted && !parent.AllTabs.Any())
                parent.Host.Close();
        }

        protected virtual void TogglePinStatusExecuted(TabModel item) { }
        protected virtual void SelectItemExecuted(TabModel item) => SelectedItem = item;
        protected virtual void FloatTabExecuted(FloatEventArgs e) { }
        protected virtual void FloatAllExecuted(FloatEventArgs e) { }

        protected virtual void DockExecuted(DockEventArgs e)
        {
            //Reverse() to preserve tab order
            var groups = e.SourceContent.OfType<TabWellModelBase>().Reverse().ToList();
            var index = e.TargetItem is not TabModel target || target.IsPinned ? 0 : Children.IndexOf(target);

            foreach (var group in groups)
            {
                var allChildren = group.Children.Reverse().ToList();
                foreach (var item in allChildren)
                {
                    group.Children.Remove(item);
                    item.IsPinned = false;
                    item.IsActive = false;

                    Children.Insert(index, item);
                }
            }

            e.SourceWindow.Close();
            IsActive = true;
            SelectedItem = Children[index];
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var tab in e.OldItems.OfType<TabModel>())
                    tab.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var tab in e.NewItems.OfType<TabModel>())
                    tab.Parent = this;
            }

            OnChildrenChanged();
        }

        protected virtual void OnChildrenChanged() { }

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

        internal override IEnumerable<TabModel> AllTabs => Children;
    }
}
