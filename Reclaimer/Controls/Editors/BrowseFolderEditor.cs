using System.Windows.Forms;

namespace Reclaimer.Controls.Editors
{
    public class BrowseFolderEditor : BrowseEditorBase
    {
        protected override void ShowDialog()
        {
            var fsd = new FolderBrowserDialog
            {
                InitialDirectory = PropertyItem.Value?.ToString()
            };

            if (fsd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                PropertyItem.Value = fsd.SelectedPath.Replace(Settings.AppBaseDirectory, ".\\");
        }
    }
}
