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

        static MenuButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MenuButton), new FrameworkPropertyMetadata(typeof(MenuButton)));
        }

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register(nameof(MenuItems), typeof(ObservableCollection<Control>), typeof(MenuButton), new PropertyMetadata(null, null));

        public ObservableCollection<Control> MenuItems
        {
            get { return (ObservableCollection<Control>)GetValue(MenuItemsProperty); }
            set { SetValue(MenuItemsProperty, value); }
        }

        public MenuButton()
        {
            MenuItems = new ObservableCollection<Control>();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            submenuPopup = Template.FindName(PART_Popup, this) as Popup;
        }

        protected override void OnClick()
        {
            base.OnClick();

            if (submenuPopup != null)
                submenuPopup.IsOpen = true;
        }
    }
}
