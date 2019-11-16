using Prism.Commands;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Models
{
    public class DockContainerModel : TabOwnerModelBase
    {
        public const double DefaultDockSize = 260d;

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
        public DelegateCommand<DockEventArgs> DockCommand { get; }

        public DockContainerModel()
        {
            CloseTabCommand = new DelegateCommand<TabModel>(CloseTabExecuted);
            TogglePinStatusCommand = new DelegateCommand<TabModel>(TogglePinStatusExecuted);
            DockCommand = new DelegateCommand<DockEventArgs>(DockExecuted);

            LeftDockItems = new ObservableCollection<TabModel>();
            TopDockItems = new ObservableCollection<TabModel>();
            RightDockItems = new ObservableCollection<TabModel>();
            BottomDockItems = new ObservableCollection<TabModel>();

            LeftDockItems.CollectionChanged += DockItems_CollectionChanged;
            TopDockItems.CollectionChanged += DockItems_CollectionChanged;
            RightDockItems.CollectionChanged += DockItems_CollectionChanged;
            BottomDockItems.CollectionChanged += DockItems_CollectionChanged;
        }

        private void CloseTabExecuted(TabModel item)
        {
            LeftDockItems?.Remove(SelectedDockItem);
            TopDockItems?.Remove(SelectedDockItem);
            RightDockItems?.Remove(SelectedDockItem);
            BottomDockItems?.Remove(SelectedDockItem);
        }

        private void TogglePinStatusExecuted(TabModel item)
        {
            item = SelectedDockItem;
            if (LeftDockItems.Contains(item))
            {
                LeftDockItems.Remove(item);
                AddTool(item, null, Dock.Left);
            }
            else if (TopDockItems.Contains(item))
            {
                TopDockItems.Remove(item);
                AddTool(item, null, Dock.Top);
            }
            else if (RightDockItems.Contains(item))
            {
                RightDockItems.Remove(item);
                AddTool(item, null, Dock.Right);
            }
            else if (BottomDockItems.Contains(item))
            {
                BottomDockItems.Remove(item);
                AddTool(item, null, Dock.Bottom);
            }
        }

        private void DockExecuted(DockEventArgs e)
        {
            var groups = e.SourceContent.OfType<TabWellModelBase>().ToList();
            var newGroup = new ToolWellModel() { Dock = (Dock)((int)e.TargetDock - 5) };

            foreach (var group in groups)
            {
                var allChildren = group.Children.ToList();
                foreach (var item in allChildren)
                {
                    group.Children.Remove(item);
                    item.IsPinned = false;
                    item.IsActive = false;

                    newGroup.Children.Add(item);
                }
            }

            var newSplit = new SplitPanelModel();
            newSplit.Orientation = e.TargetDock == DockTarget.DockLeft || e.TargetDock == DockTarget.DockRight
                ? Orientation.Horizontal
                : Orientation.Vertical;

            var existing = Content;
            Content = null;

            if (e.TargetDock == DockTarget.DockTop || e.TargetDock == DockTarget.DockLeft)
            {
                newSplit.Item1 = newGroup;
                newSplit.Item2 = existing;
                newSplit.Item1.PanelSize = new GridLength(e.DesiredSize);
            }
            else
            {
                newSplit.Item1 = existing;
                newSplit.Item2 = newGroup;
                newSplit.Item2.PanelSize = new GridLength(e.DesiredSize);
            }

            Content = newSplit;
            newGroup.IsActive = true;
            newGroup.SelectedItem = newGroup.Children.First();

            e.SourceWindow.Close();
        }

        public void AddTool(TabModel item, TabOwnerModelBase target, Dock dock)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            var group = new ToolWellModel() { Width = item.Width, Height = item.Height };
            group.Children.Add(item);

            var container = new SplitPanelModel(dock, group);

            if (target == null)
            {
                target = Content;
                Content = container;
            }
            else if (target.ParentBranch != null)
                target.ParentBranch.Replace(target, container);
            else
            {
                System.Diagnostics.Debugger.Break();
                throw new InvalidOperationException();
            }

            container.Add(target);
        }

        public void AddTool2(TabModel item, Dock targetDock, GridLength defaultSize)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (item.Usage == TabItemType.Document)
                throw new InvalidOperationException();

            var well = AllTabs.Where(t => (t.Parent as ToolWellModel)?.Dock == targetDock)
                .Select(t => t.Parent as ToolWellModel)
                .FirstOrDefault();

            if (well == null)
            {
                well = new ToolWellModel { Dock = targetDock };
                var branch = new SplitPanelModel(targetDock, well);
                well.PanelSize = defaultSize;

                var temp = content;
                Content = branch;
                branch.Add(temp);
            }

            well.Children.Add(item);
            well.SelectedItem = item;
            well.IsActive = true;
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
