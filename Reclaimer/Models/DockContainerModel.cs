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
    public class DockContainerModel : TabOwnerModelBase
    {
        internal Window Host { get; set; }

        private bool isRafted;
        public bool IsRafted
        {
            get { return isRafted; }
            internal set { SetProperty(ref isRafted, value); }
        }

        private TabModel selectedDockItem;
        public TabModel SelectedDockItem
        {
            get { return selectedDockItem; }
            set { SetProperty(ref selectedDockItem, value); }
        }

        private TabOwnerModelBase content;
        public TabOwnerModelBase Content
        {
            get { return content; }
            set { SetProperty(ref content, value, OnContentChanged); }
        }

        public ObservableCollection<TabModel> LeftDockItems { get; }
        public ObservableCollection<TabModel> TopDockItems { get; }
        public ObservableCollection<TabModel> RightDockItems { get; }
        public ObservableCollection<TabModel> BottomDockItems { get; }

        public DelegateCommand<TabModel> CloseTabCommand { get; }
        public DelegateCommand<TabModel> TogglePinStatusCommand { get; }
        //public DelegateCommand<DockEventArgs> DockCommand { get; }

        public DockContainerModel()
        {
            //CloseTabCommand = new DelegateCommand<TabItem>(CloseTabExecuted);
            //TogglePinStatusCommand = new DelegateCommand<TabItem>(TogglePinStatusExecuted);
            //DockCommand = new DelegateCommand<DockEventArgs>(DockExecuted);

            LeftDockItems = new ObservableCollection<TabModel>();
            TopDockItems = new ObservableCollection<TabModel>();
            RightDockItems = new ObservableCollection<TabModel>();
            BottomDockItems = new ObservableCollection<TabModel>();

            LeftDockItems.CollectionChanged += DockItems_CollectionChanged;
            TopDockItems.CollectionChanged += DockItems_CollectionChanged;
            RightDockItems.CollectionChanged += DockItems_CollectionChanged;
            BottomDockItems.CollectionChanged += DockItems_CollectionChanged;
        }

        private void DockItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<TabModel>())
                    item.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<TabModel>())
                    item.Parent = this;
            }
        }

        private void OnContentChanged(TabOwnerModelBase prev, TabOwnerModelBase next)
        {
            prev?.SetParent(null);
            next?.SetParent(this);
        }

        internal override IEnumerable<TabModel> AllTabs
        {
            get
            {
                var contentItems = Content?.AllTabs ?? Enumerable.Empty<TabModel>();
                return LeftDockItems.Concat(TopDockItems).Concat(RightDockItems).Concat(BottomDockItems).Concat(contentItems);
            }
        }
    }
}
