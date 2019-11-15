using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Reclaimer.Models
{
    public class SplitPanel : TabOwnerBase
    {
        private ObservableCollection<TabOwnerBase> items = new ObservableCollection<TabOwnerBase>();
        public ReadOnlyObservableCollection<TabOwnerBase> Items { get; }

        private Orientation orientation = Orientation.Horizontal;
        public Orientation Orientation
        {
            get { return orientation; }
            set { SetProperty(ref orientation, value); }
        }

        public TabOwnerBase Item1
        {
            get { return Items[0]; }
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

        public TabOwnerBase Item2
        {
            get { return Items[1]; }
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

        public SplitPanel()
        {
            items.Add(null);
            items.Add(null);
            Items = new ReadOnlyObservableCollection<TabOwnerBase>(items);
        }

        private void OnItemChanged(TabOwnerBase prev, TabOwnerBase next)
        {
            prev?.SetParent(null);
            next?.SetParent(this);

            if (next == null)
            {
                var remaining = Item1 ?? Item2;
                if (remaining == null) return;
                Item1 = Item2 = null;

                if (ParentBrach != null)
                    ParentBrach.Replace(this, remaining);
                else if (ParentContainer != null)
                    ParentContainer.Content = remaining;
            }
        }

        public bool Add(TabOwnerBase item)
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

        public bool Remove(TabOwnerBase item)
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

        public bool Replace(TabOwnerBase prev, TabOwnerBase next)
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

        internal override IEnumerable<TabItem> AllTabs
        {
            get
            {
                var tabs1 = Item1?.AllTabs ?? Enumerable.Empty<TabItem>();
                var tabs2 = Item2?.AllTabs ?? Enumerable.Empty<TabItem>();
                return tabs1.Concat(tabs2);
            }
        }
    }
}
