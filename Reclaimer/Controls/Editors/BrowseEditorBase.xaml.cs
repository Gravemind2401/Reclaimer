using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Reclaimer.Controls
{
    /// <summary>
    /// Interaction logic for BrowseEditorBase.xaml
    /// </summary>
    public abstract partial class BrowseEditorBase : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty PropertyItemProperty =
            DependencyProperty.Register(nameof(PropertyItem), typeof(PropertyItem), typeof(BrowseEditorBase));

        public PropertyItem PropertyItem
        {
            get => (PropertyItem)GetValue(PropertyItemProperty);
            set => SetValue(PropertyItemProperty, value);
        }

        public BrowseEditorBase()
        {
            InitializeComponent();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            ShowDialog();
        }

        protected virtual void ShowDialog() { }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            PropertyItem = propertyItem;
            return this;
        }
    }
}
