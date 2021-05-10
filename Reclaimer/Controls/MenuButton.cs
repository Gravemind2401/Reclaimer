using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Reclaimer.Controls
{
    [TemplatePart(Name = PART_Popup, Type = typeof(Popup))]
    public class MenuButton : Button
    {
        private const string PART_Popup = "PART_Popup";

        private Popup submenuPopup;

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register(nameof(MenuItems), typeof(ObservableCollection<Control>), typeof(MenuButton), new PropertyMetadata(new ObservableCollection<Control>()));

        public ObservableCollection<Control> MenuItems
        {
            get { return (ObservableCollection<Control>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }

        static MenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuButton), new FrameworkPropertyMetadata(typeof(MenuButton)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            submenuPopup = GetTemplateChild(PART_Popup) as Popup;
        }

        protected override void OnClick()
        {
            base.OnClick();

            if (submenuPopup != null)
                submenuPopup.IsOpen = true;
        }
    }
}
