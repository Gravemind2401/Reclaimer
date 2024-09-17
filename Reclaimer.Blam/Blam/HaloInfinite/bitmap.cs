using Reclaimer.Blam.Common;
using Reclaimer.Blam.Properties;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.IO;
using System.DirectoryServices.ActiveDirectory;
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

        int IBitmap.SubmapCount => Bitmaps[0].Type.Equals(BitmapType.Array) ? Bitmaps[0].Depth : Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => CubemapLayout.NonCubemap;



        public DdsImage ToDds(int index)
        {
            var resource = GetResource(index, out var isChunk);
            var submap = Bitmaps[0].Type.Equals(BitmapType.Array) ? Bitmaps[0] : Bitmaps[index];

            if (resource.DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1) && Item.Module.hd1Stream == null)
            {
                resource = Item.Module.Items[Item.Module.Resources[Item.ResourceIndex + Item.ResourceCount - 2]];
                submap.Width /= 2;
                submap.Height /= 2;
            }

            var format = TextureUtils.DXNSwap(submap.BitmapFormat, true);
            var props = new BitmapProperties(submap.Width, submap.Height, format, "Texture2D");
            var size = TextureUtils.GetBitmapDataLength(props, false);

            byte[] data = ReadResourceData(resource, index, isChunk, Bitmaps[0].Type.Equals(BitmapType.Array), size);
            return TextureUtils.GetDds(props, data, false);
        }

        private ModuleItem GetResource(int index, out bool isChunk)
        {
            isChunk = false;
            ModuleItem resource;

            if (Bitmaps.Count == 1 && Item.UncompressedActualResourceSize == 0)
            {
                var resourceIndex = Item.ResourceIndex + Item.ResourceCount - 1;
                resource = Item.Module.Items[Item.Module.Resources[resourceIndex] - index];
                isChunk = true;
            }
            else
            {
                resource = Item.UncompressedActualResourceSize > 0
                    ? Item
                    : Item.Module.Items[Item.Module.Resources[Item.ResourceIndex] + index];
            }

            if (resource.ResourceCount > 0)
            {
                resource = Item.Module.Items[Item.Module.Resources[resource.ResourceIndex + resource.ResourceCount - 1]];
                isChunk = true;
            }

            if (resource.UncompressedActualResourceSize > 0)
            {
                isChunk = false;
            }

            return resource;
        }

        private static byte[] ReadResourceData(ModuleItem resource, int index, bool isChunk, bool isArray, int size)
        {
            using (var reader = resource.CreateReader())
            {
                if (isChunk)
                {
                    return reader.ReadBytes(size);
                }

                if (isArray)
                {
                    reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagSize + (size * index), SeekOrigin.Begin);
                    return reader.ReadBytes(size);
                }

                if (resource.Flags.HasFlag(FileEntryFlags.HasBlocks))
                {
                    reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagSize, SeekOrigin.Begin);
                    return reader.ReadBytes((int)resource.UncompressedActualResourceSize);
                }

                return resource.Flags.HasFlag(FileEntryFlags.RawFile)
                    ? reader.ReadBytes(resource.TotalUncompressedSize)
                    : Array.Empty<byte>();
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
        [Offset(4)]
        public short Depth { get; set; }
        [Offset(6)]
        public BitmapType Type { get; set; }

        [Offset(8)]
        public TextureFormat BitmapFormat { get; set; }
    }
    public enum BitmapType : byte
    {
        Texture2D,
        Texture3D,
        CubeMap,
        Array,
    }
}