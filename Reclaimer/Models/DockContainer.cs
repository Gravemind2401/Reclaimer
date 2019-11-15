using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Models
{
    public class DockContainer : TabOwnerBase
    {
        internal Window Host { get; set; }

        private bool isRafted;
        public bool IsRafted
        {
            get { return isRafted; }
            internal set { SetProperty(ref isRafted, value); }
        }

        private TabItem selectedDockItem;
        public TabItem SelectedDockItem
        {
            get { return selectedDockItem; }
            set { SetProperty(ref selectedDockItem, value); }
        }

        private TabOwnerBase content;
        public TabOwnerBase Content
        {
            get { return content; }
            set { SetProperty(ref content, value, OnContentChanged); }
        }

        public ObservableCollection<TabItem> LeftDockItems { get; }
        public ObservableCollection<TabItem> TopDockItems { get; }
        public ObservableCollection<TabItem> RightDockItems { get; }
        public ObservableCollection<TabItem> BottomDockItems { get; }

        public DelegateCommand<TabItem> CloseTabCommand { get; }
        public DelegateCommand<TabItem> TogglePinStatusCommand { get; }
        //public DelegateCommand<DockEventArgs> DockCommand { get; }

        public DockContainer()
        {
            //CloseTabCommand = new DelegateCommand<TabItem>(CloseTabExecuted);
            //TogglePinStatusCommand = new DelegateCommand<TabItem>(TogglePinStatusExecuted);
            //DockCommand = new DelegateCommand<DockEventArgs>(DockExecuted);

            LeftDockItems = new ObservableCollection<TabItem>();
            TopDockItems = new ObservableCollection<TabItem>();
            RightDockItems = new ObservableCollection<TabItem>();
            BottomDockItems = new ObservableCollection<TabItem>();

            LeftDockItems.CollectionChanged += DockItems_CollectionChanged;
            TopDockItems.CollectionChanged += DockItems_CollectionChanged;
            RightDockItems.CollectionChanged += DockItems_CollectionChanged;
            BottomDockItems.CollectionChanged += DockItems_CollectionChanged;
        }

        private void DockItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<TabItem>())
                    item.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<TabItem>())
                    item.Parent = this;
            }
        }

        private void OnContentChanged(TabOwnerBase prev, TabOwnerBase next)
        {
            prev?.SetParent(null);
            next?.SetParent(this);
        }

        internal override IEnumerable<TabItem> AllTabs
        {
            get
            {
                var contentItems = Content?.AllTabs ?? Enumerable.Empty<TabItem>();
                return LeftDockItems.Concat(TopDockItems).Concat(RightDockItems).Concat(BottomDockItems).Concat(contentItems);
            }
        }
    }
}
