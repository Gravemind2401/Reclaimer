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

        public static BitmapSource ToBitmapSource(this DdsImage dds) => ToBitmapSource(dds, new DdsOutputArgs());
        public static BitmapSource ToBitmapSource(this DdsImage dds, DdsOutputArgs args)
        {
            using var ms = new MemoryStream();
            dds.WriteToStream(ms, ImageFormat.Png, args);

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
    }
}
