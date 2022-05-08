using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Reclaimer.Controls.Editors
{
    /// <summary>
    /// Interaction logic for MultiSelectEditorBase.xaml
    /// </summary>
    public abstract partial class MultiSelectEditorBase : UserControl, ITypeEditor
    {
        public static readonly DependencyPropertyKey OptionsPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Options), typeof(IList<Selectable>), typeof(MultiSelectEditorBase), new PropertyMetadata(null, null));

        public static readonly DependencyProperty OptionsProperty = OptionsPropertyKey.DependencyProperty;

        public static readonly DependencyProperty PropertyItemProperty =
            DependencyProperty.Register(nameof(PropertyItem), typeof(PropertyItem), typeof(MultiSelectEditorBase));

        public IList<Selectable> Options
        {
            get => (IList<Selectable>)GetValue(OptionsProperty);
            private set => SetValue(OptionsPropertyKey, value);
        }

        public PropertyItem PropertyItem
        {
            get => (PropertyItem)GetValue(PropertyItemProperty);
            set => SetValue(PropertyItemProperty, value);
        }

        public MultiSelectEditorBase()
        {
            InitializeComponent();
        }

        protected abstract IList<Selectable> GetItems(PropertyItem propertyItem);

        protected abstract void SetItems(PropertyItem propertyItem, IEnumerable<object> selectedItems);

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            PropertyItem = propertyItem;
            Options = GetItems(propertyItem);
            return this;
        }

        private void ComboBox_DropDownClosed(object sender, EventArgs e) => SetItems(PropertyItem, Options.Where(o => o.IsSelected).Select(o => o.Value));

        public class Selectable
        {
            public object Value { get; set; }
            public bool IsSelected { get; set; }
        }
    }
}
