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
    public abstract class TabOwnerModelBase : BindableBase
    {
        private TabOwnerModelBase parent;
        public TabOwnerModelBase Parent
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

        protected virtual void OnParentChanged(TabOwnerModelBase prev, TabOwnerModelBase next)
        {

        }

        internal SplitPanelModel ParentBrach => Parent as SplitPanelModel;

        internal DockContainerModel ParentContainer
        {
            get
            {
                var model = Parent;
                while (model != null)
                {
                    if (model is DockContainerModel)
                        return model as DockContainerModel;
                    else model = model.Parent;
                }

                return null;
            }
        }

        internal void SetParent(TabOwnerModelBase parent) => Parent = parent;

        internal abstract IEnumerable<TabModel> AllTabs { get; }
    }
}
