using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Forms = System.Windows.Forms;

namespace Reclaimer.Utilities
{
    public class FolderSelectDialog
    {
        private readonly Forms.OpenFileDialog ofd;

        public FolderSelectDialog()
        {
            ofd = new Forms.OpenFileDialog
            {
                Filter = "Folders|\n",
                AddExtension = false,
                CheckFileExists = false,
                DereferenceLinks = true,
                Multiselect = false
            };
        }

        /// <summary>
        /// Gets or sets the initial directory displayed by the folder dialog box.
        /// </summary>
        public string InitialDirectory
        {
            get { return ofd.InitialDirectory; }
            set { ofd.InitialDirectory = value; }
        }

        /// <summary>
        /// Gets or sets the folder dialog box title.
        /// </summary>
        public string Title
        {
            get { return ofd.Title; }
            set { ofd.Title = value; }
        }

        /// <summary>
        /// Gets the path selected by the user.
        /// </summary>
        public string SelectedPath => ofd.FileName;

        public bool ShowDialog()
        {
            return ShowDialog(null);
        }

        public bool ShowDialog(Window owner)
        {
            var instanceNonPublic = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var vistaDialog = ofd.GetType().GetMethod("CreateVistaDialog", instanceNonPublic).Invoke(ofd, null);
            ofd.GetType().GetMethod("OnBeforeVistaDialog", instanceNonPublic).Invoke(ofd, new[] { vistaDialog });

            var FileDialogNative = Assembly.GetAssembly(typeof(Forms.OpenFileDialog)).GetType("System.Windows.Forms.FileDialogNative");
            var IFileDialog = FileDialogNative.GetNestedType("IFileDialog", BindingFlags.NonPublic);

            var options = (uint)typeof(Forms.FileDialog).GetMethod("GetOptions", instanceNonPublic).Invoke(ofd, null);
            options |= (uint)FileDialogNative.GetNestedType("FOS", BindingFlags.NonPublic).GetField("FOS_PICKFOLDERS").GetValue(null);
            IFileDialog.GetMethod("SetOptions", instanceNonPublic).Invoke(vistaDialog, new object[] { options });

            var VistaDialogEvents = typeof(Forms.FileDialog).GetNestedType("VistaDialogEvents", BindingFlags.NonPublic);
            var events = Activator.CreateInstance(VistaDialogEvents, ofd);

            var adviseParams = new object[] { events, (uint)0 };
            IFileDialog.GetMethod("Advise", instanceNonPublic).Invoke(vistaDialog, adviseParams);

            var adviseResult = (uint)adviseParams[1];

            bool showResult;
            try
            {
                var handle = owner == null ? IntPtr.Zero : new System.Windows.Interop.WindowInteropHelper(owner).Handle;
                showResult = 0 == (int)IFileDialog.GetMethod("Show", instanceNonPublic).Invoke(vistaDialog, new object[] { handle });
            }
            finally
            {
                IFileDialog.GetMethod("Unadvise", instanceNonPublic).Invoke(vistaDialog, new object[] { adviseResult });
                GC.KeepAlive(events);
            }

            return showResult;
        }
    }
}
