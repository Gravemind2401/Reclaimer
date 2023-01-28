using Reclaimer.Blam.Utilities;

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
            }

            content = null;
            return false;
        }

        public static bool TryGetBitmapContent(IPakItem item, out IBitmap content)
        {
            content = null;

            if (item.ItemType != PakItemType.Textures)
                return false;

            content = new Halo1X.Texture((Halo1X.PakItem)item);

            return content != null;
        }
    }
}
