using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Models
{
    public class TabItem : BindableBase
    {
        private TabOwnerBase parent;
        public TabOwnerBase Parent
        {
            get { return parent; }
            internal set { SetProperty(ref parent, value); }
        }

        //private TabItemType usage;
        //public TabItemType Usage
        //{
        //    get { return usage; }
        //    set { SetProperty(ref usage, value); }
        //}

        private bool isPinned;
        public bool IsPinned
        {
            get { return isPinned; }
            set { SetProperty(ref isPinned, value); }
        }

        private bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set { SetProperty(ref isActive, value); }
        }

        private string header;
        public string Header
        {
            get { return header; }
            set { SetProperty(ref header, value); }
        }

        private string toolTip;
        public string ToolTip
        {
            get { return toolTip; }
            set { SetProperty(ref toolTip, value); }
        }

        internal double Width { get; set; }
        internal double Height { get; set; }

        private FrameworkElement content;
        public FrameworkElement Content
        {
            get { return content; }
            set { SetProperty(ref content, value); }
        }
    }
}
