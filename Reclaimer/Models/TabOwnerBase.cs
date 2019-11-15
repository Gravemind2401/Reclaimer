using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Models
{
    public abstract class TabOwnerBase : BindableBase
    {
        private TabOwnerBase parent;
        public TabOwnerBase Parent
        {
            get { return parent; }
            set { SetProperty(ref parent, value, OnParentChanged); }
        }

        private GridLength panelSize = new GridLength(1, GridUnitType.Star);
        public GridLength PanelSize
        {
            get { return panelSize; }
            set { SetProperty(ref panelSize, value); }
        }

        private double minPanelSize = 65d;
        public double MinPanelSize
        {
            get { return minPanelSize; }
            set { SetProperty(ref minPanelSize, value); }
        }

        protected bool SetProperty<T>(ref T storage, T value, Action<T, T> onChanged, [CallerMemberName]string propertyName = null)
        {
            var prev = storage;
            if (SetProperty(ref storage, value, propertyName))
            {
                onChanged(prev, value);
                return true;
            }
            else return false;
        }

        protected virtual void OnParentChanged(TabOwnerBase prev, TabOwnerBase next)
        {

        }

        internal SplitPanel ParentBrach => Parent as SplitPanel;

        internal DockContainer ParentContainer
        {
            get
            {
                var model = Parent;
                while (model != null)
                {
                    if (model is DockContainer)
                        return model as DockContainer;
                    else model = model.Parent;
                }

                return null;
            }
        }

        internal void SetParent(TabOwnerBase parent) => Parent = parent;

        internal abstract IEnumerable<TabItem> AllTabs { get; }
    }
}
