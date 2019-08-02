using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utilities
{
    public class TreeNode : BindableBase
    {
        private string header;
        public string Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        private object tag;
        public object Tag
        {
            get { return tag; }
            set { SetProperty(ref tag, value); }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set { SetProperty(ref isExpanded, value); }
        }
        
        public bool HasChildren => Children?.Count > 0;

        private ObservableCollection<TreeNode> children;
        public ObservableCollection<TreeNode> Children
        {
            get { return children; }
            set
            {
                var prev = children;
                if (SetProperty(ref children, value))
                    OnChildrenChanged(prev, value);
            }
        }

        public TreeNode()
        {
            Children = new ObservableCollection<TreeNode>();
        }

        public TreeNode(string header) : this()
        {
            Header = header;
        }

        private void OnChildrenChanged(ObservableCollection<TreeNode> prev, ObservableCollection<TreeNode> next)
        {
            if (prev != null)
                prev.CollectionChanged -= Children_CollectionChanged;

            if (next != null)
                next.CollectionChanged += Children_CollectionChanged;

            RaisePropertyChanged(nameof(HasChildren));
        }

        private void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RaisePropertyChanged(nameof(HasChildren));
        }
    }
}
