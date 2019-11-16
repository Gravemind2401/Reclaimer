using Prism.Mvvm;
using Studio.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Models
{
    public class TabModel : BindableBase
    {
        private TabOwnerModelBase parent;
        public TabOwnerModelBase Parent
        {
            get { return parent; }
            internal set { SetProperty(ref parent, value); }
        }

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

        private double width;
        public double Width
        {
            get { return width; }
            set { SetProperty(ref width, value); }
        }

        private double height;
        public double Height
        {
            get { return height; }
            set { SetProperty(ref height, value); }
        }

        public FrameworkElement Content { get; }
        public TabItemType Usage { get; }

        public TabModel(FrameworkElement content, TabItemType usage)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            Content = content;
            Usage = usage;
        }
    }
}
