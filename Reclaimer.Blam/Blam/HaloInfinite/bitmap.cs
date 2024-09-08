using Reclaimer.Blam.Common;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class bitmap : ContentTagDefinition<IBitmap>, IBitmap
    {
        public bitmap(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(212)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        #region IContentProvider

        public override IBitmap GetContent() => this;

        int IBitmap.SubmapCount => Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => CubemapLayout.NonCubemap;

        

        public DdsImage ToDds(int index)
        {
            var resourceIndex = Item.ResourceIndex + Item.ResourceCount - 1;
            var resource = Item.Module.Items[Item.Module.Resources[resourceIndex] - index];
            var submap = Bitmaps[index];

            // Skip to the mip after the empty one from HD1 if HD1 module isn't loaded.
            if (resource.DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1) && Item.Module.hd1Stream == null)
            {
                resourceIndex--;
                resource = Item.Module.Items[Item.Module.Resources[resourceIndex]];
                submap.Width /= 2;
                submap.Height /= 2;
            }

            if (!resource.Flags.HasFlag(FileEntryFlags.RawFile) && resource.UncompressedActualResourceSize == 0)
            {
                resource = Item.Module.Items[Item.Module.Resources[resource.ResourceIndex + resource.ResourceCount - 1]];
            }

            byte[] data = ReadResourceData(index, resource);

            var format = TextureUtils.DXNSwap(submap.BitmapFormat, true);
            var props = new BitmapProperties(submap.Width, submap.Height, format, "Texture2D");
            return TextureUtils.GetDds(props, data, false);
        }

        private byte[] ReadResourceData(int index, ModuleItem resource)
        {
            using (var reader = (index < Item.ResourceCount)
                                        ? resource.CreateReader()
                                        : Item.CreateReader())
            {
                if (resource.Flags.HasFlag(FileEntryFlags.HasBlocks) && Item.UncompressedActualResourceSize == 0 && resource.UncompressedActualResourceSize == 0)
                {
                    reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagSize, SeekOrigin.Begin);
                }
                else if (index >= Item.ResourceCount && Item.UncompressedActualResourceSize > 0)
                {
                    reader.Seek(Item.UncompressedHeaderSize + Item.UncompressedTagSize, SeekOrigin.Begin);
                    // Early return if bitmap data is contained inside tag.
                    return reader.ReadBytes((int)Item.UncompressedActualResourceSize);
                }
                else if (resource.UncompressedActualResourceSize > 0 && resource.Flags.HasFlag(FileEntryFlags.HasBlocks))
                {
                    reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagSize, SeekOrigin.Begin);
                    // Early return if bitmap data is contained inside resource.
                    return reader.ReadBytes((int)resource.UncompressedActualResourceSize);
                }

                return reader.ReadBytes((int)resource.TotalUncompressedSize);
            }
        }

        #endregion
    }

    [FixedSize(40)]
    public class BitmapDataBlock
    {
        [Offset(0)]
        public short Width { get; set; }

        [Offset(2)]
        public short Height { get; set; }

        [Offset(8)]
        public TextureFormat BitmapFormat { get; set; }
    }
}