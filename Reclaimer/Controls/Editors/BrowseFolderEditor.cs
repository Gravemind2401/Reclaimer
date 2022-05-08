using Reclaimer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Reclaimer.Controls.Editors
{
    public class BrowseFolderEditor : BrowseEditorBase
    {
        protected override void ShowDialog()
        {
            var ofd = new FolderSelectDialog
            {
                InitialDirectory = PropertyItem.Value?.ToString()
            };

            if (ofd.ShowDialog(Application.Current.MainWindow) == true)
                PropertyItem.Value = ofd.SelectedPath.Replace(Settings.AppBaseDirectory, ".\\");
        }
    }
}
