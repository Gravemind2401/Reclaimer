using Prism.Commands;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Reclaimer.Models
{
    public class DocumentPanelModel : TabOwnerModelBase
    {
        public ObservableCollection<DocumentWellModel> Children { get; }

        private Orientation orientation;
        public Orientation Orientation
        {
            get { return orientation; }
            set { SetProperty(ref orientation, value); }
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
