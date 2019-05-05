using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Reclaimer.Controls
{
    public class ControlBase : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (property == null && value == null)
                return false;

            if (property != null && value != null && property.Equals(value))
                return false;

            property = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T property, T value, [CallerMemberName] string propertyName = null)
        {
            if (property == null && value == null)
                return false;

            if (property != null && value != null && property.Equals(value))
                return false;

            property = value;
            RaisePropertyChanged(propertyName);
            return true;
        }

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

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
