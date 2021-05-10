using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Controls
{
    public class MenuButtonItemsControl : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is MenuItem || item is Separator;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new MenuItem();
        }

        protected override bool ShouldApplyItemContainerStyle(DependencyObject container, object item)
        {
            if (item is Separator)
                return false;

            return base.ShouldApplyItemContainerStyle(container, item);
        }
    }
}
