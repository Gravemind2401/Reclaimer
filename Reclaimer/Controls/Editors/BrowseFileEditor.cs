using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace Reclaimer.Controls.Editors
{
    public class BrowseFileEditor : BrowseEditorBase
    {
        protected override void ShowDialog()
        {
            var dir = string.Empty;
            try
            {
                dir = Directory.GetParent(PropertyItem.Value?.ToString()).FullName;
            }
            catch { }

            var ofd = new OpenFileDialog
            {
                InitialDirectory = dir,
                Multiselect = false,
                CheckFileExists = true
            };

            if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                PropertyItem.Value = ofd.FileName.Replace(Settings.AppBaseDirectory, ".\\");
        }
    }
}
