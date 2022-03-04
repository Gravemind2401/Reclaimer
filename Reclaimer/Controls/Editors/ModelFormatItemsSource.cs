using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reclaimer.Plugins;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Reclaimer.Controls.Editors
{
    public class ModelFormatItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            var items = ModelViewerPlugin.GetExportFormats()
                .Select(id => new Item
                {
                    Value = id,
                    DisplayName = ModelViewerPlugin.GetFormatDescription(id)
                })
                .OrderBy(i => i.DisplayName);

            var result = new ItemCollection();
            result.AddRange(items);

            return result;
        }
    }
}
