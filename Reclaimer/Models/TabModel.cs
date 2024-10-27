using Studio.Controls;
using System.Windows;

namespace Reclaimer.Models
{
    public class TabModel : BindableBase, IDisposable
    {
        private TabOwnerModelBase parent;
        public TabOwnerModelBase Parent
        {
            get => parent;
            internal set => SetProperty(ref parent, value);
        }

        private string contentId;
        public string ContentId
        {
            get => contentId;
            set => SetProperty(ref contentId, value);
        }

        private bool isPinned;
        public bool IsPinned
        {
            get => isPinned;
            set => SetProperty(ref isPinned, value);
        }

        private bool isActive;
        public bool IsActive
        {
            get => isActive;
            set => SetProperty(ref isActive, value);
        }

        private string header;
        public string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }

        private string toolTip;
        public string ToolTip
        {
            get => toolTip;
            set => SetProperty(ref toolTip, value);
        }

        private double width;
        public double Width
        {
            get => width;
            set => SetProperty(ref width, value);
        }

        private double height;
        public double Height
        {
            get => height;
            set => SetProperty(ref height, value);
        }

        public FrameworkElement Content { get; }
        public TabItemType Usage { get; }

        public TabModel(FrameworkElement content, TabItemType usage)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Usage = usage;
        }

        public void Dispose() => (Content as IDisposable)?.Dispose();
    }
}
