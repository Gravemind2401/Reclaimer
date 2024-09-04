using OodleSharp;
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
            var isChunk = false;

            if (index >= Item.ResourceCount)
                System.Diagnostics.Debugger.Break();

            var resourceIndex = Item.ResourceIndex + Item.ResourceCount - 1;
            var resource = Item.Module.Items[Item.Module.Resources[resourceIndex]];
            var submap = Bitmaps[index];

            // Skip to the mip after the empty one from HD1.
            // Will implement HD1 soon.
            if (resource.DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1))
            {
                resourceIndex -= 1;
                resource = Item.Module.Items[Item.Module.Resources[resourceIndex]];
                submap.Width = (short)(submap.Width / 2);
                submap.Height = (short)(submap.Height / 2);
            }
            

            byte[] data;
            using (var reader = resource.CreateReader())
            {
                if (!isChunk)
                {
                    if (resource.Flags.HasFlag(FileEntryFlags.HasBlocks))
                        reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagSize, SeekOrigin.Begin);
                }

                data = reader.ReadBytes((int)resource.TotalUncompressedSize);
            }

            //todo: cubemap check
            var format = TextureUtils.DXNSwap(submap.BitmapFormat, true);
            var props = new BitmapProperties(submap.Width, submap.Height, format, "Texture2D");
            return TextureUtils.GetDds(props, data, false);
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
