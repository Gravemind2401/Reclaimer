using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Reclaimer.Controls
{
    [TemplatePart(Name = PART_ItemsHost, Type = typeof(ItemsControl))]
    public class AutoCompleteTextBox : TextBox
    {
        private const string PART_ItemsHost = "PART_ItemsHost";

        private const int DefaultSearchDelay = 300;
        private readonly DispatcherTimer CallbackTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DefaultSearchDelay) };

        private ItemsControl ItemsHost;
        private string LastSearch;
        private bool isWorking;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        #region Dependency Properties
        private static readonly DependencyPropertyKey HasTextPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(nameof(HasText), typeof(bool), typeof(AutoCompleteTextBox), new PropertyMetadata(false, null, CoerceHasText));

        public static readonly DependencyProperty HasTextProperty = HasTextPropertyKey.DependencyProperty;

        public static readonly DependencyProperty WatermarkTextProperty =
            DependencyProperty.Register(nameof(WatermarkText), typeof(string), typeof(AutoCompleteTextBox), new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty LiveSearchDelayProperty =
            DependencyProperty.Register(nameof(CallbackDelay), typeof(int), typeof(AutoCompleteTextBox), new PropertyMetadata(DefaultSearchDelay, LiveSearchTimeoutChanged), ValidateLiveSearchDelay);

        public static readonly DependencyProperty MaxDropDownHeightProperty =
            DependencyProperty.Register(nameof(MaxDropDownHeight), typeof(double), typeof(AutoCompleteTextBox), new PropertyMetadata(300d));

        public static readonly DependencyProperty IsDropDownOpenProperty =
             DependencyProperty.Register(nameof(IsDropDownOpen), typeof(bool), typeof(AutoCompleteTextBox), new PropertyMetadata(false));

        public static readonly DependencyProperty SuggestionProviderProperty =
            DependencyProperty.Register(nameof(SuggestionProvider), typeof(ISuggestionProvider), typeof(AutoCompleteTextBox), new PropertyMetadata(null, null));

        public bool HasText
        {
            get { return (bool)GetValue(HasTextProperty); }
        }

        public string WatermarkText
        {
            get { return (string)GetValue(WatermarkTextProperty); }
            set { SetValue(WatermarkTextProperty, value); }
        }

        public int CallbackDelay
        {
            get { return (int)GetValue(LiveSearchDelayProperty); }
            set { SetValue(LiveSearchDelayProperty, value); }
        }

        public double MaxDropDownHeight
        {
            get { return (double)GetValue(MaxDropDownHeightProperty); }
            set { SetValue(MaxDropDownHeightProperty, value); }
        }

        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        public ISuggestionProvider SuggestionProvider
        {
            get { return (ISuggestionProvider)GetValue(SuggestionProviderProperty); }
            set { SetValue(SuggestionProviderProperty, value); }
        }

        public static object CoerceHasText(DependencyObject d, object baseValue)
        {
            return !string.IsNullOrEmpty((d as AutoCompleteTextBox)?.Text);
        }

        public static bool ValidateLiveSearchDelay(object value)
        {
            return value is int && (int)value >= 0;
        }

        public static void LiveSearchTimeoutChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var searchBox = d as AutoCompleteTextBox;
            searchBox.CallbackTimer.Interval = TimeSpan.FromMilliseconds((int)e.NewValue);
        }
        #endregion

        public AutoCompleteTextBox()
        {
            Loaded += AutoCompleteTextBox_Loaded;
        }

        private void AutoCompleteTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= AutoCompleteTextBox_Loaded;
            Unloaded += AutoCompleteTextBox_Unloaded;

            CallbackTimer.Tick += CallbackTimer_Tick;
            TextChanged += AutoCompleteTextBox_TextChanged;
            CoerceValue(HasTextProperty);
        }

        private void AutoCompleteTextBox_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded -= AutoCompleteTextBox_Unloaded;
            Loaded += AutoCompleteTextBox_Loaded;

            CallbackTimer.Tick -= CallbackTimer_Tick;
            TextChanged -= AutoCompleteTextBox_TextChanged;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OnTemplateUnset();
            ItemsHost = Template.FindName(PART_ItemsHost, this) as ItemsControl;
            OnTemplateSet();
        }

        private void OnTemplateUnset()
        {
            if (ItemsHost != null)
                ItemsHost.PreviewMouseUp -= ItemsHost_MouseUp;
        }

        private void OnTemplateSet()
        {
            if (ItemsHost != null)
                ItemsHost.PreviewMouseUp += ItemsHost_MouseUp;
        }

        private void ItemsHost_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is TextBlock textblock)
            {
                isWorking = true;
                Text = textblock.Text;
                Reset();
                CaretIndex = Text.Length;
                isWorking = false;
            }
        }

        private void UpdateSuggestions()
        {
            CallbackTimer.Stop();

            if (string.Equals(Text, LastSearch, StringComparison.Ordinal))
                return;

            LastSearch = Text;

            var suggestions = SuggestionProvider.GetSuggestions(Text);
            if (suggestions.Any())
            {
                ItemsHost.ItemsSource = suggestions;
                IsDropDownOpen = true;
            }
            else IsDropDownOpen = false;
        }

        private void Reset()
        {
            CallbackTimer.Stop();

            LastSearch = null;
            IsDropDownOpen = false;
            ItemsHost.ItemsSource = null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Handled) return;

            if (e.Key == Key.Escape || string.IsNullOrEmpty(Text) && e.Key == Key.Enter)
                Reset();
            else if (e.Key == Key.Enter && SuggestionProvider != null)
                UpdateSuggestions();
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);

            if (!e.Handled)
                Reset();
        }

        private void AutoCompleteTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CoerceValue(HasTextProperty);
            CallbackTimer.Stop();

            if (IsReadOnly || !IsKeyboardFocusWithin || SuggestionProvider == null || isWorking)
                return;

            if (CallbackDelay == 0)
                CallbackTimer_Tick(null, null);
            else CallbackTimer.Start();
        }

        private void CallbackTimer_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
                Reset();
            else UpdateSuggestions();
        }

        public interface ISuggestionProvider
        {
            IEnumerable<string> GetSuggestions(string text);
        }
    }
}
