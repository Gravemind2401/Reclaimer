using Reclaimer.Utilities;
using Reclaimer.Windows;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Models
{
    public class DocumentWellModel : TabWellModelBase
    {
        public DocumentPanelModel ParentPanel => Parent as DocumentPanelModel;

        protected override void TogglePinStatusExecuted(TabModel item)
        {
            item.IsPinned = !item.IsPinned;
        }

        protected override void FloatTabExecuted(FloatEventArgs e)
        {
            var item = e.DataContext as TabModel;
            Children.Remove(item);

            var model = new DockContainerModel { IsRafted = true };
            var well = new DocumentWellModel();
            well.Children.Add(item);

            var panel = new DocumentPanelModel(well);
            model.Content = panel;

            var wnd = new RaftedWindow(model, panel)
            {
                Left = e.VisualBounds.X,
                Top = e.VisualBounds.Y,
                Width = e.VisualBounds.Width,
                Height = e.VisualBounds.Height
            };

            if (ParentContainer != null && ParentContainer.IsRafted && !ParentContainer.AllTabs.Any())
                ParentContainer.Host.TransitionTo(wnd);
            else
            {
                wnd.Show();
                wnd.DragMove();
            }
        }

        protected override void DockExecuted(DockEventArgs e)
        {
            if (e.TargetDock == DockTarget.Center)
                base.DockExecuted(e);
            else if (e.TargetDock == DockTarget.SplitLeft || e.TargetDock == DockTarget.SplitTop || e.TargetDock == DockTarget.SplitRight || e.TargetDock == DockTarget.SplitBottom)
                InnerDock(e);
            else OuterDock(e);
        }

        private void InnerDock(DockEventArgs e)
        {
            var index = ParentPanel.Children.IndexOf(this);

            if (e.TargetDock == DockTarget.SplitRight || e.TargetDock == DockTarget.SplitBottom)
                index++;

            var orientation = e.TargetDock == DockTarget.SplitLeft || e.TargetDock == DockTarget.SplitRight
                ? Orientation.Horizontal
                : Orientation.Vertical;

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

            ParentPanel.Orientation = orientation;
            ParentPanel.Children.Insert(index, newGroup);

            foreach (var child in ParentPanel.Children)
                child.PanelSize = new GridLength(1, GridUnitType.Star);

            e.SourceWindow.Close();
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

            if (ParentPanel.ParentBranch == null)
                ParentContainer.Content = newSplit;
            else
                ParentPanel.ParentBranch.Replace(ParentPanel, newSplit);

            if (e.TargetDock == DockTarget.DockTop || e.TargetDock == DockTarget.DockLeft)
            {
                newSplit.Item1 = newGroup;
                newSplit.Item2 = ParentPanel;
                newSplit.Item1.PanelSize = new GridLength(e.DesiredSize);
            }
            else
            {
                newSplit.Item1 = ParentPanel;
                newSplit.Item2 = newGroup;
                newSplit.Item2.PanelSize = new GridLength(e.DesiredSize);
            }

            newGroup.IsActive = true;
            newGroup.SelectedItem = newGroup.Children.First();

            e.SourceWindow.Close();
        }

        protected override void OnChildrenChanged()
        {
            if (Children.Count == 0)
                ParentPanel?.Children.Remove(this);
        }
    }
}
