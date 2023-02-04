using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Controls
{
    public class MenuButtonItemsControl : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item) => item is MenuItem || item is Separator;
        protected override DependencyObject GetContainerForItemOverride() => new MenuItem();
        protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
        {
            return item is not Separator && base.ShouldApplyItemContainerStyle(container, item);
        }
    }
}
