using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Prism.Commands;
using Studio.Controls;

namespace Reclaimer.Models
{
    public class DocumentPanelModel : TabOwnerModelBase
    {
        public ObservableCollection<DocumentWellModel> Children { get; }

        private Orientation orientation;
        public Orientation Orientation
        {
            get => orientation;
            set => SetProperty(ref orientation, value);
        }

        public DelegateCommand<DockEventArgs> DockCommand { get; }

        public DocumentPanelModel()
        {
            DockCommand = new DelegateCommand<DockEventArgs>(DockExecuted);

            Children = new ObservableCollection<DocumentWellModel>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        public DocumentPanelModel(DocumentWellModel child) : this()
        {
            Children.Add(child);
        }

        public void AddItem(TabModel item)
        {
            var container = Children.OrderByDescending(c => c.IsActive).FirstOrDefault();
            if (container == null)
            {
                container = new DocumentWellModel();
                Children.Add(container);
            }

            container.Children.Insert(0, item);
            container.SelectedItem = item;
            container.IsActive = true;
        }

        private void DockExecuted(DockEventArgs e)
        {
            //dock can only be center or outer - no splits
            if (e.TargetDock != DockTarget.Center)
            {
                OuterDock(e);
                return;
            }

            var groups = e.SourceContent.OfType<TabWellModelBase>().ToList();
            var newGroup = new DocumentWellModel();

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

            e.SourceWindow.Close();
            Children.Add(newGroup);
            newGroup.IsActive = true;
            newGroup.SelectedItem = newGroup.Children[0];
        }

        private void OuterDock(DockEventArgs e)
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

            if (ParentBranch == null)
                ParentContainer.Content = newSplit;
            else
                ParentBranch.Replace(this, newSplit);

            if (e.TargetDock == DockTarget.DockTop || e.TargetDock == DockTarget.DockLeft)
            {
                newSplit.Item1 = newGroup;
                newSplit.Item2 = this;
                newSplit.Item1.PanelSize = new GridLength(e.DesiredSize);
            }
            else
            {
                newSplit.Item1 = this;
                newSplit.Item2 = newGroup;
                newSplit.Item2.PanelSize = new GridLength(e.DesiredSize);
            }

            newGroup.IsActive = true;
            newGroup.SelectedItem = newGroup.Children.First();

            e.SourceWindow.Close();
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<DocumentWellModel>())
                    item.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<DocumentWellModel>())
                    item.Parent = this;
            }
        }

        internal override IEnumerable<TabModel> AllTabs => Children.SelectMany(c => c.Children);
    }
}
