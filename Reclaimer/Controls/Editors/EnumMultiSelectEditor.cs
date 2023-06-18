using System.Collections;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace Reclaimer.Controls.Editors
{
    public class EnumMultiSelectEditor : MultiSelectEditorBase
    {
        private static Type GetTargetType(PropertyItem propertyItem)
        {
            var targetType = propertyItem.PropertyDescriptor.PropertyType
                .GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                ?.GenericTypeArguments.FirstOrDefault();

            return targetType != null && targetType.IsEnum ? targetType : null;
        }

        protected override IList<Selectable> GetItems(PropertyItem propertyItem)
        {
            var result = new List<Selectable>();

            var targetType = GetTargetType(propertyItem);
            if (targetType == null)
                return result;

            var selected = (PropertyItem.Value as IEnumerable)?.OfType<object>();

            foreach (var option in Enum.GetValues(targetType))
                result.Add(new Selectable { Value = option, IsSelected = selected?.Contains(option) ?? false });

            return result;
        }

        protected override void SetItems(PropertyItem propertyItem, IEnumerable<object> selectedItems)
        {
            var targetType = GetTargetType(propertyItem);
            if (targetType == null)
                return;

            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(targetType));

            foreach (var item in selectedItems)
                list.Add(item);

            PropertyItem.Value = list;
        }
    }
}
