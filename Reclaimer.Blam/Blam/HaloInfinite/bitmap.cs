using Reclaimer.Blam.Common.Gen5;
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

        int IBitmap.SubmapCount => Bitmaps[0].BitmapType.Equals(TextureType.Array) ? Bitmaps[0].Depth : Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => CubemapLayout.NonCubemap;

        public DdsImage ToDds(int index)
        {
            var resource = GetResource(index, out var isChunk);
            var submap = Bitmaps[0].BitmapType.Equals(TextureType.Array) ? Bitmaps[0] : Bitmaps[index];

            var format = TextureUtils.DXNSwap(submap.BitmapFormat, true);
            var props = new BitmapProperties(submap.Width, submap.Height, format, "Texture2D");
            var size = TextureUtils.GetBitmapDataLength(props, false);
            var data = ReadResourceData(resource, index, isChunk, Bitmaps[0].BitmapType.Equals(TextureType.Array), size);
            return TextureUtils.GetDds(props, data, false);
        }

        private ModuleItem GetResource(int index, out bool isChunk)
        {
            isChunk = false;
            ModuleItem resource;
            var submap = Bitmaps[0].BitmapType.Equals(TextureType.Array) ? Bitmaps[0] : Bitmaps[index];

            // If bitmap has resource data in itself, use that
            // Otherwise, use the resource indexed by the submap
            if (Bitmaps.Count == 1 && Item.UncompressedActualResourceSize == 0)
            {
                var resourceIndex = Item.ResourceIndex + Item.ResourceCount - 1;
                resource = Item.Module.Items[Item.Module.Resources[resourceIndex] - index];
                isChunk = true;
            }
            else
            {
                // This means that the bitmap itself has metadata, either as resource data or tag blocks
                resource = Item.UncompressedActualResourceSize > 0
                    ? Item
                    : Item.Module.Items[Item.Module.Resources[Item.ResourceIndex] + index];
            }

            if (resource.DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1) && Item.Module.Hd1Stream == null)
            {
                resource = Item.Module.Items[Item.Module.Resources[Item.ResourceIndex + Item.ResourceCount - 2] - index];
                submap.Width /= 2;
                submap.Height /= 2;
            }
                

            if (resource.ResourceCount > 0)
            {
                var newResource = Item.Module.Items[Item.Module.Resources[resource.ResourceIndex + resource.ResourceCount - 1]];
                var resourceMetadata = resource.ReadMetadata<BitmapDataResource>()?.Data;
                var blockData = resourceMetadata[^1];

                if (newResource.DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1) && Item.Module.Hd1Stream == null)
                {
                    resource = Item.Module.Items[Item.Module.Resources[resource.ResourceIndex + resource.ResourceCount - 2]];
                    blockData = resourceMetadata[^2];
                }
                else
                    resource = newResource;

                /* This is so incredibly stupid, but for some reason bitmaps MAY contain incorrect dimensions!
                 * I want to talk to whoever thought this was a good idea.
                 * Maybe it's a way to crop bitmaps in-engine? whatever
                 * To fix this, we need to check if the dimensions are correct and if not, we need to calculate them.
                 * Width/Height is split into two shorts, so we bit shift to get the correct values.
                 */
                var height = blockData.Dimensions >> 16;
                var width = blockData.Dimensions & 0xFFFF;
                var bytesPerPixel = blockData.Size / (height * width);

                if (bytesPerPixel == 2 || bytesPerPixel == 1)
                {
                    submap.Width = (short)(blockData.Size / (height * bytesPerPixel));
                    submap.Height = (short)(blockData.Size / (width * bytesPerPixel));
                }

                isChunk = true;
            }

            if (resource.UncompressedActualResourceSize > 0)
                isChunk = false;

            return resource;
        }

        private static byte[] ReadResourceData(ModuleItem resource, int index, bool isChunk, bool isArray, int size)
        {
            using (var reader = resource.CreateReader())
            {
                if (isChunk)
                    return reader.ReadBytes(size);

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
        public TextureType BitmapType { get; set; }

        [Offset(8)]
        public TextureFormat BitmapFormat { get; set; }
    }

    [FixedSize(72)]
    public class BitmapDataResource
    {
        [Offset(48)]
        public BlockCollection<StreamingBitmapData> Data { get; set; }
    }

    [FixedSize(16)]
    public class StreamingBitmapData
    {
        [Offset(0)]
        public int Offset { get; set; }

        [Offset(4)]
        public int Size { get; set; }

        [Offset(8)]
        public int ChunkInfo { get; set; }

        [Offset(12)]
        public int Dimensions { get; set; }
    }

    public enum TextureType : byte
    {
        Texture2D,
        Texture3D,
        CubeMap,
        Array,
    }
}