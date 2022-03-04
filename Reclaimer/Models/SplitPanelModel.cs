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
    public class SplitPanelModel : TabOwnerModelBase
    {
        private readonly ObservableCollection<TabOwnerModelBase> items = new ObservableCollection<TabOwnerModelBase>();
        public ReadOnlyObservableCollection<TabOwnerModelBase> Items { get; }

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get => orientation;
            set => SetProperty(ref orientation, value);
        }

        public TabOwnerModelBase Item1
        {
            get => Items[0];
            set
            {
                if (Item1 == value)
                    return;

                var prev = Item1;
                items[0] = value;

                RaisePropertyChanged();
                OnItemChanged(prev, value);
            }
        }

        public TabOwnerModelBase Item2
        {
            get => Items[1];
            set
            {
                if (Item2 == value)
                    return;

                var prev = Item2;
                items[1] = value;

                RaisePropertyChanged();
                OnItemChanged(prev, value);
            }
        }

        public SplitPanelModel()
        {
            items.Add(null);
            items.Add(null);
            Items = new ReadOnlyObservableCollection<TabOwnerModelBase>(items);
        }

        public SplitPanelModel(Dock dock, TabWellModelBase content) : this()
        {
            Orientation = dock == Dock.Left || dock == Dock.Right ? Orientation.Horizontal : Orientation.Vertical;

            if (dock == Dock.Left || dock == Dock.Top)
            {
                Item1 = content;
                Item1.PanelSize = new GridLength(dock == Dock.Left ? content.Width : content.Height);
                Item2 = null;
            }
            else
            {
                Item1 = null;
                Item2 = content;
                Item2.PanelSize = new GridLength(dock == Dock.Right ? content.Width : content.Height);
            }
        }

        private void OnItemChanged(TabOwnerModelBase prev, TabOwnerModelBase next)
        {
            prev?.SetParent(null);
            next?.SetParent(this);

            if (next == null)
            {
                var remaining = Item1 ?? Item2;
                if (remaining == null) return;
                Item1 = Item2 = null;

                if (ParentBranch != null)
                    ParentBranch.Replace(this, remaining);
                else if (ParentContainer != null)
                    ParentContainer.Content = remaining;
            }
        }

        public bool Add(TabOwnerModelBase item)
        {
            if (Item1 == null)
            {
                Item1 = item;
                return true;
            }
            else if (Item2 == null)
            {
                Item2 = item;
                return true;
            }

            return false;
        }

        public bool Remove(TabOwnerModelBase item)
        {
            if (Item1 == item)
            {
                Item1 = null;
                return true;
            }
            else if (Item2 == item)
            {
                Item2 = null;
                return true;
            }

            return false;
        }

        public bool Replace(TabOwnerModelBase prev, TabOwnerModelBase next)
        {
            if (Item1 == prev)
            {
                var sizeTemp = Item1.PanelSize;
                Item1 = next;
                Item1.PanelSize = sizeTemp;
                return true;
            }
            else if (Item2 == prev)
            {
                var sizeTemp = Item2.PanelSize;
                Item2 = next;
                Item2.PanelSize = sizeTemp;
                return true;
            }

            return false;
        }

        internal override IEnumerable<TabModel> AllTabs
        {
            get
            {
                var tabs1 = Item1?.AllTabs ?? Enumerable.Empty<TabModel>();
                var tabs2 = Item2?.AllTabs ?? Enumerable.Empty<TabModel>();
                return tabs1.Concat(tabs2);
            }
        }
    }
}
