using Reclaimer.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Xml;

namespace Reclaimer.Utilities
{
    public static class Extensions
    {
        public static int FirstIndexWhere<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            var index = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    return index;
                index++;
            }

            return -1;
        }

        public static int LastIndexWhere<TSource>(this IEnumerable<TSource> source, Predicate<TSource> predicate)
        {
            var match = -1;
            var index = 0;

            foreach (var item in source)
            {
                if (predicate(item))
                    match = index;
                index++;
            }

            return match;
        }

        public static IEnumerable<string> SplitPath(this DirectoryInfo directory)
        {
            while (directory != null)
            {
                yield return directory.Name;
                directory = directory.Parent;
            }
        }

        public static void TransitionTo(this Window prev, Window next)
        {
            //if we try to close window A then show and drag window B we get the error 'Can only call DragMove when primary mouse button is down.'
            //this is a workaround to ensure window A has closed before window B attempts to show and drag
            prev.Closed += (s, e) =>
            {
                next.Show();
                next.DragMove();
            };
            prev.Close();
        }

        public static IEnumerable<XmlNode> GetChildElements(this XmlNode node)
        {
            return node.ChildNodes.OfType<XmlNode>().Where(n => n.NodeType == XmlNodeType.Element);
        }

        public static void WriteToDxgi(this DdsImage dds, string fileName)
        {
            if (dds.FormatCode == (int)FourCC.XBOX)
                dds = dds.AsUncompressed();
            dds.WriteToDisk(fileName);
        }

        public static void WriteToTarga(this DdsImage dds, string fileName, DdsOutputArgs args)
        {
            var source = dds.ToBitmapSource(args);
            var format = args.Options.HasFlag(DecompressOptions.Bgr24) ? System.Drawing.Imaging.PixelFormat.Format24bppRgb : System.Drawing.Imaging.PixelFormat.Format32bppArgb;
            WriteToTarga(source, fileName, format);
        }

        public static void WriteToTarga(this BitmapSource source, string fileName, System.Drawing.Imaging.PixelFormat format)
        {
            var dest = new System.Drawing.Bitmap(source.PixelWidth, source.PixelHeight, format);
            var destData = dest.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, dest.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, format);
            source.CopyPixels(Int32Rect.Empty, destData.Scan0, destData.Height * destData.Stride, destData.Stride);
            dest.UnlockBits(destData);

            var tga = new TGASharpLib.TGA(dest);
            tga.Save(fileName);
        }
    }
}
