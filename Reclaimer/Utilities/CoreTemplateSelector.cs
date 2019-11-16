using Reclaimer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Utilities
{
    public class CoreTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            if (item is SplitPanelModel)
                return element.FindResource("SplitPanelTemplate") as DataTemplate;
            else if (item is DocumentPanelModel)
                return element.FindResource("DocumentPanelTemplate") as DataTemplate;
            else if (item is ToolWellModel)
                return element.FindResource("ToolWellTemplate") as DataTemplate;
            else
                return base.SelectTemplate(item, container);
        }
    }
}
