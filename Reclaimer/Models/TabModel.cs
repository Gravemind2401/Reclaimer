using Studio.Controls;
using System.Windows;

namespace Reclaimer.Models
{
    public class TabModel : BindableBase, IDisposable
    {
        public TabOwnerModelBase Parent
        {
            get;
            internal set => SetProperty(ref field, value);
        }

        public string ContentId
        {
            get;
            set => SetProperty(ref field, value);
        }

        public bool IsPinned
        {
            get;
            set => SetProperty(ref field, value);
        }

        public bool IsActive
        {
            get;
            set => SetProperty(ref field, value);
        }

        public string Header
        {
            get;
            set => SetProperty(ref field, value);
        }

        public string ToolTip
        {
            get;
            set => SetProperty(ref field, value);
        }

        public double Width
        {
            get;
            set => SetProperty(ref field, value);
        }

        public double Height
        {
            get;
            set => SetProperty(ref field, value);
        }

        public FrameworkElement Content { get; }
        public TabItemType Usage { get; }

        public TabModel(FrameworkElement content, TabItemType usage)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            Usage = usage;
        }

        public void Close()
        {
            var closeCommand = (Parent as TabWellModelBase)?.CloseTabCommand
                ?? Parent.ParentContainer.CloseTabCommand;

            closeCommand.Execute(this);
        }

        public void Dispose() => (Content as IDisposable)?.Dispose();
    }
}
