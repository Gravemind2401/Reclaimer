using Reclaimer.Utilities;
using System.Windows;

namespace Reclaimer.Models
{
    [DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
    public class TreeItemModel : BindableBase
    {
        public DelegateCommand ToggleCommand { get; }
        public BulkObservableCollection<TreeItemModel> Items { get; }

        public bool HasItems => Items.Count > 0;

        private TreeItemModel parent;
        public TreeItemModel Parent
        {
            get => parent;
            private set => SetProperty(ref parent, value);
        }

        private string header;
        public string Header
        {
            get => header;
            set => SetProperty(ref header, value);
        }

        private bool? isChecked;
        public bool? IsChecked
        {
            get => isChecked;
            set => SetProperty(ref isChecked, value);
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set => SetProperty(ref isExpanded, value);
        }

        private int itemType;
        public int ItemType
        {
            get => itemType;
            set => SetProperty(ref itemType, value);
        }

        private object tag;
        public object Tag
        {
            get => tag;
            set => SetProperty(ref tag, value);
        }

        public bool IsVisible => Visibility == Visibility.Visible;

        private Visibility visibility;
        public Visibility Visibility
        {
            get => visibility;
            set
            {
                if (SetProperty(ref visibility, value))
                    RaisePropertyChanged(nameof(IsVisible));
            }
        }

        public TreeItemModel()
        {
            ToggleCommand = new DelegateCommand(() => IsChecked = !IsChecked.GetValueOrDefault(true));
            Items = new BulkObservableCollection<TreeItemModel>();
            Items.CollectionChanged += Items_CollectionChanged;
        }

        public TreeItemModel(string header) : this()
        {
            Header = header;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<TreeItemModel>())
                    item.Parent = null;
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<TreeItemModel>())
                {
                    if (item.Parent != null)
                        throw new InvalidOperationException();

                    item.Parent = this;
                }
            }

            RaisePropertyChanged(nameof(HasItems));
        }

        public void ExpandAll() => ExpandAll(true);

        public void CollapseAll() => ExpandAll(false);

        public void ExpandAll(bool expand)
        {
            foreach (var n in Items)
                n.CollapseAll();
            IsExpanded = expand;
        }

        /// <summary>
        /// Returns <see langword="this"/>, followed by all descendants recursively.
        /// </summary>
        public IEnumerable<TreeItemModel> EnumerateHierarchy() => EnumerateHierarchy(null);

        /// <param name="predicate">
        /// Items will be skipped (including their descendants) if the predicate returns <see langword="false"/>.
        /// </param>
        /// <inheritdoc cref="EnumerateHierarchy()"/>
        public IEnumerable<TreeItemModel> EnumerateHierarchy(Predicate<TreeItemModel> predicate)
        {
            return predicate?.Invoke(this) == false
                ? Enumerable.Empty<TreeItemModel>()
                : Items.SelectMany(x => x.EnumerateHierarchy(predicate)).Prepend(this);
        }

        private string GetDebuggerDisplay() => Header ?? ToString();
    }
}
