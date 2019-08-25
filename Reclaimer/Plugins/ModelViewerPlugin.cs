using Adjutant.Blam.Common;
using Adjutant.Utilities;
using Reclaimer.Utilities;
using Reclaimer.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Reclaimer.Plugins
{
    public class ModelViewerPlugin : Plugin
    {
        public override string Name => "Model Viewer";

        public override bool CanOpenFile(OpenFileArgs args)
        {
            return args.File.Any(i => i is IRenderGeometry);
        }

        public override void OpenFile(OpenFileArgs args)
        {
            var model = args.File.OfType<IRenderGeometry>().FirstOrDefault();
            var container = args.TargetWindow.DocumentContainer;

            LogOutput($"Loading model: {args.FileName}");

            try
            {
                var viewer = new Controls.ModelViewer();
                viewer.LoadGeometry(model, $"{args.FileName}");

                container.Items.Add(viewer); ;

                LogOutput($"Loaded model: {args.FileName}");
            }
            catch (Exception e)
            {
                LogError($"Error loading model: {args.FileName}", e);
            }
        }
    }
}
