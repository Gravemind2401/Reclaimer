using Reclaimer.Models;
using System.Windows;
using System.Windows.Controls;

namespace Reclaimer.Utilities
{
    public class CoreTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;
            var templateName = item switch
            {
                SplitPanelModel => "SplitPanelTemplate",
                DocumentPanelModel => "DocumentPanelTemplate",
                ToolWellModel => "ToolWellTemplate",
                _ => null
            };

            return templateName == null
                ? base.SelectTemplate(item, container)
                : element.FindResource(templateName) as DataTemplate;
        }
    }
}
