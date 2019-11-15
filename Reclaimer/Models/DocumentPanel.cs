using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Reclaimer.Models
{
    public class DocumentPanel : TabOwnerBase
    {
        public ObservableCollection<DocumentWell> Children { get; }

        private Orientation orientation;
        public Orientation Orientation
        {
            get { return orientation; }
            set { SetProperty(ref orientation, value); }
        }

        //public DelegateCommand<DockEventArgs> DockCommand { get; }

        public DocumentPanel()
        {
            //DockCommand = new DelegateCommand<DockEventArgs>(DockExecuted);

            Children = new ObservableCollection<DocumentWell>();
            Children.CollectionChanged += Children_CollectionChanged;
        }

        public DocumentPanel(DocumentWell child) : this()
        {
            Children.Add(child);
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<DocumentWell>())
                    item.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<DocumentWell>())
                    item.Parent = this;
            }
        }

        internal override IEnumerable<TabItem> AllTabs => Children.SelectMany(c => c.Children);
    }
}
