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
    public class ToolWellModel : TabWellModelBase
    {
        private bool isWindow;
        public bool IsWindow
        {
            get { return isWindow; }
            internal set { SetProperty(ref isWindow, value); }
        }

        private Dock dock;
        public Dock Dock
        {
            get { return dock; }
            internal set { SetProperty(ref dock, value); }
        }

        protected override void TogglePinStatusExecuted(TabModel _)
        {
            var container = ParentContainer; //keep an instance because it will get nulled at the end
            foreach (var item in Children.ToList())
            {
                Children.Remove(item);
                item.IsActive = true;

                if (Dock == Dock.Left)
                    container.LeftDockItems.Add(item);
                else if (Dock == Dock.Top)
                    container.TopDockItems.Add(item);
                else if (Dock == Dock.Right)
                    container.RightDockItems.Add(item);
                else if (Dock == Dock.Bottom)
                    container.BottomDockItems.Add(item);
            }
        }

        protected override void FloatTabExecuted(FloatEventArgs e)
        {
            var item = e.DataContext as TabModel;
            Children.Remove(item);

            var group = new ToolWellModel() { IsWindow = true };
            group.Children.Add(item);

            var wnd = new ToolWindow
            {
                Content = group,
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

        protected override void FloatAllExecuted(FloatEventArgs e)
        {
            var pc = ParentContainer;

            Remove();
            IsWindow = true;

            var wnd = new ToolWindow
            {
                Content = this,
                Left = e.VisualBounds.X,
                Top = e.VisualBounds.Y,
                Width = e.VisualBounds.Width,
                Height = e.VisualBounds.Height
            };

            if (pc.Host != null && !pc.AllTabs.Any())
                pc.Host.TransitionTo(wnd);
            else
            {
                wnd.Show();
                wnd.DragMove();
            }
        }

        protected override void DockExecuted(DockEventArgs e)
        {
            if (e.TargetDock == DockTarget.Center)
            {
                base.DockExecuted(e);
                return;
            }

            var groups = e.SourceContent.OfType<TabWellModelBase>().ToList();
            var newGroup = new ToolWellModel() { Dock = Dock };

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

            double remainingSize;
            if (e.TargetDock == DockTarget.SplitLeft || e.TargetDock == DockTarget.SplitRight)
            {
                newSplit.Orientation = Orientation.Horizontal;
                remainingSize = Width - e.DesiredSize;
            }
            else
            {
                newSplit.Orientation = Orientation.Vertical;
                remainingSize = Height - e.DesiredSize;
            }

            ParentBranch.Replace(this, newSplit);
            if (e.TargetDock == DockTarget.SplitTop || e.TargetDock == DockTarget.SplitLeft)
            {
                newSplit.Item1 = newGroup;
                newSplit.Item2 = this;
                newSplit.Item1.PanelSize = new GridLength(e.DesiredSize, GridUnitType.Star);
                newSplit.Item2.PanelSize = new GridLength(remainingSize, GridUnitType.Star);
            }
            else
            {
                newSplit.Item1 = this;
                newSplit.Item2 = newGroup;
                newSplit.Item1.PanelSize = new GridLength(remainingSize, GridUnitType.Star);
                newSplit.Item2.PanelSize = new GridLength(e.DesiredSize, GridUnitType.Star);
            }

            newGroup.IsActive = true;
            newGroup.SelectedItem = newGroup.Children.First();

            e.SourceWindow.Close();
        }

        protected override void OnChildrenChanged()
        {
            if (Children.Count == 0)
                Remove();
        }

        internal void Remove()
        {
            if (ParentBranch != null)
                ParentBranch.Remove(this);
            else if (ParentContainer != null)
                ParentContainer.Content = null;
        }
    }
}
