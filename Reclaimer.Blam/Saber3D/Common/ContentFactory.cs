using Reclaimer.Drawing;
using Reclaimer.Utilities;

namespace Reclaimer.Saber3D.Common
{
    public static class ContentFactory
    {
        public static bool TryGetPrimaryContent(IPakItem item, out object content)
        {
            switch (item.ItemType)
            {
                case PakItemType.Textures:
                    if (TryGetBitmapContent(item, out var bitmapContent))
                    {
                        content = bitmapContent;
                        return true;
                    }
                    break;
                case PakItemType.Templates:
                case PakItemType.Scene:
                    if (TryGetGeometryContent(item, out var geometryContent))
                    {
                        content = geometryContent;
                        return true;
                    }
                    break;
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(IPakItem item, out IContentProvider<IBitmap> content)
        {
            content = null;

            if (item.ItemType != PakItemType.Textures)
                return false;

            content = new Halo1X.Texture((Halo1X.PakItem)item);

            return content != null;
        }

        public static bool TryGetGeometryContent(IPakItem item, out IContentProvider<Geometry.Scene> content)
        {
            content = null;

            if (item.ItemType == PakItemType.Templates)
                content = new Halo1X.Template((Halo1X.PakItem)item);
            else if (item.ItemType == PakItemType.Scene)
                content = new Halo1X.Scene((Halo1X.PakItem)item);

            return content != null;
        }
    }
}
