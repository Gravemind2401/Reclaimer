using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reclaimer.Utils
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

        private ObservableCollection<TreeNode> children;
        public ObservableCollection<TreeNode> Children
        {
            get { return children; }
            set { SetProperty(ref children, value); }
        }

        public TreeNode()
        {
            Children = new ObservableCollection<TreeNode>();
        }
    }
}
