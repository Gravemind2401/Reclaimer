using System;
using System.Activities.Presentation.Converters;
using System.Activities.Presentation.Model;
using System.Activities.Presentation.PropertyEditing;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Reclaimer.Utilities
{
    public class BrowseFolderEditor : DialogPropertyValueEditor
    {
        public BrowseFolderEditor()
        {
            string template = @"
                <DataTemplate
                    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
                    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
                    xmlns:pe='clr-namespace:System.Activities.Presentation.PropertyEditing;assembly=System.Activities.Presentation'>
                    <DockPanel LastChildFill='True'>
                        <pe:EditModeSwitchButton TargetEditMode='Dialog' Name='EditButton'
                        HorizontalContentAlignment='Center'
                        DockPanel.Dock='Right'>...</pe:EditModeSwitchButton>
                        <TextBlock Text='{Binding Path=Value}' ToolTip='{Binding Path=Value}' Margin='2,0,0,0' VerticalAlignment='Center'/>
                    </DockPanel>
                </DataTemplate>";

            using (var sr = new MemoryStream(Encoding.UTF8.GetBytes(template)))
            {
                InlineEditorTemplate = XamlReader.Load(sr) as DataTemplate;
            }
        }

        public override void ShowDialog(PropertyValue propertyValue, IInputElement commandSource)
        {
            var ofd = new FolderSelectDialog
            {
                InitialDirectory = propertyValue.Value?.ToString()
            };

            if (ofd.ShowDialog(Application.Current.MainWindow) == true)
            {
                var ownerActivityConverter = new ModelPropertyEntryToOwnerActivityConverter();
                ModelItem activityItem = ownerActivityConverter.Convert(propertyValue.ParentProperty, typeof(ModelItem), false, null) as ModelItem;
                using (ModelEditingScope editingScope = activityItem.BeginEdit())
                {
                    propertyValue.Value = ofd.SelectedPath.Replace(AppDomain.CurrentDomain.BaseDirectory, ".\\");
                    editingScope.Complete(); // commit the changes

                    var control = commandSource as Control;
                    var oldData = control.DataContext;
                    control.DataContext = null;
                    control.DataContext = oldData;
                }
            }
        }
    }
}
