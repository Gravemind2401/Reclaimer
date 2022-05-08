// *********************************************************************
// PLEASE DO NOT REMOVE THIS DISCLAIMER
//
// WpfPropertyGrid - By Jaime Olivares
// July 11, 2011
// Article site: http://www.codeproject.com/KB/grid/WpfPropertyGrid.aspx
// Author site: www.jaimeolivares.com
// License: Code Project Open License (CPOL)
//
// *********************************************************************

using System;
using System.Activities.Presentation;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.View;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Reclaimer.Controls
{
    public enum PropertySort
    {
        NoSort = 0,
        Alphabetical = 1,
        Categorized = 2,
        CategorizedAlphabetical = 3
    };

    /// <summary>WPF Native PropertyGrid class, uses Workflow Foundation's PropertyInspector</summary>
    public class WpfPropertyGrid : Grid
    {
        #region Private fields
        private readonly WorkflowDesigner Designer;
        private readonly MethodInfo RefreshMethod;
        private readonly MethodInfo OnSelectionChangedMethod;
        private readonly MethodInfo IsInAlphaViewMethod;
        private readonly TextBlock SelectionTypeLabel;
        private readonly Control PropertyToolBar;
        private readonly Border HelpText;
        private readonly GridSplitter Splitter;
        private double HelpTextHeight = 60;
        #endregion

        #region Public properties
        /// <summary>Get or sets the selected object. Can be null.</summary>
        public object SelectedObject
        {
            get => GetValue(SelectedObjectProperty);
            set => SetValue(SelectedObjectProperty, value);
        }
        /// <summary>Get or sets the selected object collection. Returns empty array by default.</summary>
        public object[] SelectedObjects
        {
            get => GetValue(SelectedObjectsProperty) as object[];
            set => SetValue(SelectedObjectsProperty, value);
        }
        /// <summary>XAML information with PropertyGrid's font and color information</summary>
        /// <seealso>Documentation for WorkflowDesigner.PropertyInspectorFontAndColorData</seealso>
        public string FontAndColorData
        {
            set => Designer.PropertyInspectorFontAndColorData = value;
        }
        /// <summary>Shows the description area on the top of the control</summary>
        public bool HelpVisible
        {
            get => (bool)GetValue(HelpVisibleProperty);
            set => SetValue(HelpVisibleProperty, value);
        }
        /// <summary>Shows the tolbar on the top of the control</summary>
        public bool ToolbarVisible
        {
            get => (bool)GetValue(ToolbarVisibleProperty);
            set => SetValue(ToolbarVisibleProperty, value);
        }
        /// <summary>Shows the type label on the top of the control</summary>
        public bool TypeLabelVisible
        {
            get => (bool)GetValue(TypeLabelVisibleProperty);
            set => SetValue(TypeLabelVisibleProperty, value);
        }
        public PropertySort PropertySort
        {
            get => (PropertySort)GetValue(PropertySortProperty);
            set => SetValue(PropertySortProperty, value);
        }
        #endregion

        #region Dependency properties registration
        public static readonly DependencyProperty SelectedObjectProperty =
            DependencyProperty.Register("SelectedObject", typeof(object), typeof(WpfPropertyGrid),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedObjectPropertyChanged));

        public static readonly DependencyProperty SelectedObjectsProperty =
            DependencyProperty.Register("SelectedObjects", typeof(object[]), typeof(WpfPropertyGrid),
            new FrameworkPropertyMetadata(new object[0], FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, SelectedObjectsPropertyChanged, CoerceSelectedObjects));

        public static readonly DependencyProperty HelpVisibleProperty =
            DependencyProperty.Register("HelpVisible", typeof(bool), typeof(WpfPropertyGrid),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, HelpVisiblePropertyChanged));
        public static readonly DependencyProperty ToolbarVisibleProperty =
            DependencyProperty.Register("ToolbarVisible", typeof(bool), typeof(WpfPropertyGrid),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ToolbarVisiblePropertyChanged));
        public static readonly DependencyProperty PropertySortProperty =
            DependencyProperty.Register("PropertySort", typeof(PropertySort), typeof(WpfPropertyGrid),
            new FrameworkPropertyMetadata(PropertySort.CategorizedAlphabetical, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertySortPropertyChanged));
        public static readonly DependencyProperty TypeLabelVisibleProperty =
            DependencyProperty.Register("TypeLabelVisible", typeof(bool), typeof(WpfPropertyGrid),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, TypeLabelVisiblePropertyChanged));

        #endregion

        #region Dependency properties events
        //private static object CoerceSelectedObject(DependencyObject d, object value)
        //{
        //    var pg = d as WpfPropertyGrid;
        //    var collection = pg.GetValue(SelectedObjectsProperty) as object[];
        //
        //    return collection.Length == 0 ? null : value;
        //}
        private static object CoerceSelectedObjects(DependencyObject d, object value)
        {
            var pg = d as WpfPropertyGrid;
            var single = pg.GetValue(SelectedObjectsProperty);

            return single == null ? new object[0] : value;
        }

        private static void SelectedObjectPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var pg = source as WpfPropertyGrid;
            pg.CoerceValue(SelectedObjectsProperty);

            if (e.NewValue == null)
            {
                pg.OnSelectionChangedMethod.Invoke(pg.Designer.PropertyInspectorView, new object[] { null });
                pg.SelectionTypeLabel.Text = string.Empty;
            }
            else
            {
                var context = new EditingContext();
                var mtm = new ModelTreeManager(context);
                mtm.Load(e.NewValue);
                var selection = Selection.Select(context, mtm.Root);

                pg.OnSelectionChangedMethod.Invoke(pg.Designer.PropertyInspectorView, new object[] { selection });
                pg.SelectionTypeLabel.Text = e.NewValue.GetType().Name;
            }

            pg.ChangeHelpText(string.Empty, string.Empty);
        }
        private static void SelectedObjectsPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var pg = source as WpfPropertyGrid;
            pg.CoerceValue(SelectedObjectsProperty);

            var collection = e.NewValue as object[];

            if (collection.Length == 0)
            {
                pg.OnSelectionChangedMethod.Invoke(pg.Designer.PropertyInspectorView, new object[] { null });
                pg.SelectionTypeLabel.Text = string.Empty;
            }
            else
            {
                var same = true;
                Type first = null;

                var context = new EditingContext();
                var mtm = new ModelTreeManager(context);
                Selection selection = null;

                // Accumulates the selection and determines the type to be shown in the top of the PG
                for (var i = 0; i < collection.Length; i++)
                {
                    mtm.Load(collection[i]);
                    if (i == 0)
                    {
                        selection = Selection.Select(context, mtm.Root);
                        first = collection[0].GetType();
                    }
                    else
                    {
                        selection = Selection.Union(context, mtm.Root);
                        if (!collection[i].GetType().Equals(first))
                            same = false;
                    }
                }

                pg.OnSelectionChangedMethod.Invoke(pg.Designer.PropertyInspectorView, new object[] { selection });
                pg.SelectionTypeLabel.Text = same ? first.Name + " <multiple>" : "Object <multiple>";
            }

            pg.ChangeHelpText(string.Empty, string.Empty);
        }
        private static void HelpVisiblePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var pg = source as WpfPropertyGrid;

            if (e.NewValue != e.OldValue)
            {
                if (e.NewValue.Equals(true))
                {
                    pg.RowDefinitions[1].Height = new GridLength(5);
                    pg.RowDefinitions[2].Height = new GridLength(pg.HelpTextHeight);
                }
                else
                {
                    pg.HelpTextHeight = pg.RowDefinitions[2].Height.Value;
                    pg.RowDefinitions[1].Height = new GridLength(0);
                    pg.RowDefinitions[2].Height = new GridLength(0);
                }
            }
        }
        private static void ToolbarVisiblePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var pg = source as WpfPropertyGrid;
            pg.PropertyToolBar.Visibility = e.NewValue.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
        }
        private static void TypeLabelVisiblePropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var pg = source as WpfPropertyGrid;
            ((FrameworkElement)pg.SelectionTypeLabel.Parent).Visibility = e.NewValue.Equals(true) ? Visibility.Visible : Visibility.Collapsed;
        }
        private static void PropertySortPropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var pg = source as WpfPropertyGrid;
            var sort = (PropertySort)e.NewValue;

            var isAlpha = sort == PropertySort.Alphabetical || sort == PropertySort.NoSort;
            pg.IsInAlphaViewMethod.Invoke(pg.Designer.PropertyInspectorView, new object[] { isAlpha });
        }
        #endregion

        /// <summary>Default constructor, creates the UIElements including a PropertyInspector</summary>
        public WpfPropertyGrid()
        {
            ColumnDefinitions.Add(new ColumnDefinition());
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0) });
            RowDefinitions.Add(new RowDefinition() { Height = new GridLength(0) });

            Designer = new WorkflowDesigner();
            var title = new TextBlock()
            {
                Visibility = Visibility.Visible,
                TextWrapping = TextWrapping.NoWrap,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontWeight = FontWeights.Bold
            };
            var descrip = new TextBlock()
            {
                Visibility = Visibility.Visible,
                TextWrapping = TextWrapping.Wrap,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            var dock = new DockPanel()
            {
                Visibility = Visibility.Visible,
                LastChildFill = true,
                Margin = new Thickness(3, 0, 3, 0)
            };

            title.SetValue(DockPanel.DockProperty, Dock.Top);
            dock.Children.Add(title);
            dock.Children.Add(descrip);
            HelpText = new Border()
            {
                Visibility = Visibility.Visible,
                BorderBrush = SystemColors.ActiveBorderBrush,
                Background = SystemColors.ControlBrush,
                BorderThickness = new Thickness(1),
                Child = dock
            };
            Splitter = new GridSplitter()
            {
                Visibility = Visibility.Visible,
                ResizeDirection = GridResizeDirection.Rows,
                Height = 5,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            var inspector = Designer.PropertyInspectorView;
            inspector.Visibility = Visibility.Visible;
            inspector.SetValue(VerticalAlignmentProperty, VerticalAlignment.Stretch);

            Splitter.SetValue(RowProperty, 1);
            Splitter.SetValue(ColumnProperty, 0);

            HelpText.SetValue(RowProperty, 2);
            HelpText.SetValue(ColumnProperty, 0);

            var binding = new Binding("Parent.Background");
            title.SetBinding(BackgroundProperty, binding);
            descrip.SetBinding(BackgroundProperty, binding);

            Children.Add(inspector);
            Children.Add(Splitter);
            Children.Add(HelpText);

            var inspectorType = inspector.GetType();
            //var props = inspectorType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            //var methods = inspectorType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            RefreshMethod = inspectorType.GetMethod("RefreshPropertyList", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            IsInAlphaViewMethod = inspectorType.GetMethod("set_IsInAlphaView", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            OnSelectionChangedMethod = inspectorType.GetMethod("OnSelectionChanged", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            SelectionTypeLabel = inspectorType.GetMethod("get_SelectionTypeLabel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Invoke(inspector, new object[0]) as TextBlock;
            PropertyToolBar = inspectorType.GetMethod("get_PropertyToolBar", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly).Invoke(inspector, new object[0]) as Control;
            inspectorType.GetEvent("GotFocus").AddEventHandler(this, Delegate.CreateDelegate(typeof(RoutedEventHandler), this, "GotFocusHandler", false));

            SelectionTypeLabel.Text = string.Empty;
        }

        /// <summary>Updates the PropertyGrid's properties</summary>
        public void RefreshPropertyList() => RefreshMethod.Invoke(Designer.PropertyInspectorView, new object[] { false });

        /// <summary>Traps the change of focused property and updates the help text</summary>
        /// <param name="sender">Not used</param>
        /// <param name="args">Points to the source control containing the selected property</param>
        private void GotFocusHandler(object sender, RoutedEventArgs args)
        {
            //if (args.OriginalSource is TextBlock)
            //{
            var title = string.Empty;
            var descrip = string.Empty;

            if (GetValue(SelectedObjectsProperty) is object[] theSelectedObjects && theSelectedObjects.Length > 0)
            {
                var first = theSelectedObjects[0].GetType();
                for (var i = 1; i < theSelectedObjects.Length; i++)
                {
                    if (!theSelectedObjects[i].GetType().Equals(first))
                    {
                        ChangeHelpText(title, descrip);
                        return;
                    }
                }

                var data = (args.OriginalSource as FrameworkElement).DataContext;
                var propEntry = data.GetType().GetProperty("PropertyEntry")
                    ?? data.GetType().GetProperty("ParentProperty");

                if (propEntry != null)
                {
                    var propEntryValue = propEntry.GetValue(data, null);
                    var propName = propEntryValue.GetType().GetProperty("PropertyName").GetValue(propEntryValue, null) as string;
                    title = propEntryValue.GetType().GetProperty("DisplayName").GetValue(propEntryValue, null) as string;
                    var property = theSelectedObjects[0].GetType().GetProperty(propName);
                    var attrs = property.GetCustomAttributes(typeof(DescriptionAttribute), true);

                    if (attrs != null && attrs.Length > 0)
                        descrip = (attrs[0] as DescriptionAttribute).Description;
                }
                ChangeHelpText(title, descrip);
            }
            //}
        }

        /// <summary>Changes the text help area contents</summary>
        /// <param name="title">Title in bold</param>
        /// <param name="descrip">Description with ellipsis</param>
        private void ChangeHelpText(string title, string descrip)
        {
            var dock = HelpText.Child as DockPanel;
            (dock.Children[0] as TextBlock).Text = title;
            (dock.Children[1] as TextBlock).Text = descrip;
        }
    }
}