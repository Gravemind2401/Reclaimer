using Reclaimer.Blam.Common.Gen5;
using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.IO;
using System.IO;

namespace Reclaimer.Blam.HaloInfinite
{
    public class BitmapTag : ContentTagDefinition<IBitmap>, IBitmap
    {
        public BitmapTag(ModuleItem item, MetadataHeader header)
            : base(item, header)
        { }

        [Offset(212)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        #region IContentProvider

        public override IBitmap GetContent() => this;

        int IBitmap.SubmapCount => Bitmaps?.Count > 0 && Bitmaps[0].BitmapType == TextureType.Array
                                    ? Bitmaps[0].Depth
                                    : Bitmaps?.Count ?? 0;

        CubemapLayout IBitmap.CubeLayout => CubemapLayout.NonCubemap;

        private record class ResourceDimensions(ModuleItem Resource, int Height, int Width, bool HD1Available);

        public DdsImage ToDds(int index)
        {
            var submap = GetSubmap(index);
            var resourceDimensions = GetResourceAndDimensions(index);

            // mip0 (hd1) to mip1 is always half res.
            var croppedWidth = resourceDimensions.HD1Available ? submap.Width : submap.Width / 2;
            var croppedHeight = resourceDimensions.HD1Available ? submap.Height : submap.Height / 2;

            var format = TextureUtils.DXNSwap(submap.BitmapFormat, true);
            var props = new BitmapProperties(croppedWidth, croppedHeight, format, "Texture2D")
            {
                VirtualHeight = resourceDimensions.Height,
                VirtualWidth = resourceDimensions.Width
            };

            var size = TextureUtils.GetBitmapDataLength(props, true);
            var data = ReadResourceData(resourceDimensions.Resource, index, Bitmaps[0].BitmapType == TextureType.Array, size);
            return TextureUtils.GetDds(props, data, false);
        }

        private BitmapDataBlock GetSubmap(int index) =>
            Bitmaps[0].BitmapType == TextureType.Array
                ? Bitmaps[0]
                : Bitmaps[index];

        private static (short height, short width) CalculateBitmapDimensions(uint size, short initialHeight, short initialWidth)
        {
            /*
             * In rare circumstances, the dimensions reported by the bitmap block may be wrong. I've observed this in mainly BC7 bitmaps.
             * My belief is that this is a way to hack in cropping in-engine, without having to recompile tags.
             * This function simply tries out every value that *might* work. It's unfortunate that its required, but at least it works.
             */
            var bytesPerPixel = (double)size / (initialHeight * initialWidth);

            // Try adjusting width first
            var newWidth = Math.Round(initialWidth * bytesPerPixel);
            if (initialHeight * newWidth == size)
                return (initialHeight, (short)newWidth);

            // Try adjusting height if width adjustment didn't work
            var newHeight = Math.Round(initialHeight * bytesPerPixel);
            if (initialWidth * newHeight == size)
                return ((short)newHeight, initialWidth);

            // Try adjusting both dimensions if individual adjustments didn't work
            if (newHeight * newWidth == size)
                return ((short)newHeight, (short)newWidth);

            // If no calculations worked, return original dimensions
            return (initialHeight, initialWidth);
        }

        private ResourceDimensions GetResourceAndDimensions(int index)
        {
            ModuleItem resource;

            var submap = GetSubmap(index);
            var hd1Available = true;
            var dimensions = (submap.Height, submap.Width);

            // Get bitmap descriptor from the last resource in the array (highest mip).
            if (Bitmaps.Count == 1 && Item.UncompressedActualResourceSize == 0)
            {
                var resourceIndex = Item.ResourceIndex + Item.ResourceCount - 1;
                resource = Item.Module.Items[Item.Module.Resources[resourceIndex] - index];
            }
            else
            {
                // Some bitmaps have data inside them, so we select that OR for array (sequenced) bitmaps grab the indexed resource.
                resource = Item.UncompressedActualResourceSize > 0 
                    ? Item : Item.Module.Items[Item.Module.Resources[Item.ResourceIndex] + index];
            }

            // HD1 textures need their resolutions halved seperately if the HD1 stream is not available.
            if (resource.DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1) && resource.Module.Hd1Stream == null)
            {
                resource = Item.Module.Items[Item.Module.Resources[Item.ResourceIndex + Item.ResourceCount - 2] - index];
                dimensions.Width /= 2;
                dimensions.Height /= 2;
                hd1Available = false;
            }

            if (resource.UncompressedActualResourceSize > 0)
            {
                // Array (sequenced) bitmaps may also have actual resource data inside them!
                var resourceData = resource.ReadMetadata<BitmapDataResource>();

                // Only re-calculate dimensions if there is more than one mip!
                // TODO: Check if there's even textures that have 1 mip.
                // If there is, implement https://en.wikipedia.org/wiki/Ostrich_algorithm
                if (resourceData.MipCountPerArraySlice == 1)
                    dimensions = CalculateBitmapDimensions(resource.UncompressedActualResourceSize, dimensions.Height, dimensions.Width);

                return new ResourceDimensions(resource, dimensions.Height, dimensions.Width, hd1Available);
            }

            // This means that the resource we extracted is a bitmap data resource, not a block with data!
            if (resource.ResourceCount > 0)
            {
                // Block that actually contains data.
                var newResource = Item.Module.Items[Item.Module.Resources[resource.ResourceIndex + resource.ResourceCount - 1]];

                // Blocks however can specify if they use HD1, so we handle that.
                if (newResource.DataOffsetFlags.HasFlag(DataOffsetFlags.UseHD1) && resource.Module.Hd1Stream == null)
                {
                    resource = Item.Module.Items[Item.Module.Resources[resource.ResourceIndex + resource.ResourceCount - 2]];
                    dimensions.Width /= 2;
                    dimensions.Height /= 2;
                    hd1Available = false;
                }
                else
                    resource = newResource;

                // At this point, we have a block (aka "RawFile") so we can simply pass the total size.
                dimensions = CalculateBitmapDimensions((uint)resource.TotalUncompressedSize, dimensions.Height, dimensions.Width);
                return new ResourceDimensions(resource, dimensions.Height, dimensions.Width, hd1Available);
            }


            if (resource.Flags.HasFlag(FileEntryFlags.RawFile))
                dimensions = CalculateBitmapDimensions((uint)resource.TotalUncompressedSize, dimensions.Height, dimensions.Width);

            return new ResourceDimensions(resource, dimensions.Height, dimensions.Width, hd1Available);
        }

        private static byte[] ReadResourceData(ModuleItem resource, int index, bool isArray, int size)
        {
            using (var reader = resource.CreateReader())
            {
                if (isArray)
                {
                    // Array bitmaps are interesting because they also contain metadata attached. It's not anything relevant to extracting them, though.
                    // All frames are stored in a single block, so we precalculate the size depending on dimensions and index through it.
                    reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagSize + (size * index), SeekOrigin.Begin);
                    return reader.ReadBytes(size);
                }

                // Plain boring blocks
                if (resource.Flags.HasFlag(FileEntryFlags.RawFile))
                    return reader.ReadBytes(resource.TotalUncompressedSize);

                // Skip over metadata in resource
                reader.Seek(resource.UncompressedHeaderSize + resource.UncompressedTagSize, SeekOrigin.Begin);
                return reader.ReadBytes((int)resource.UncompressedActualResourceSize);
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
        [Offset(33)]
        public byte MipCountPerArraySlice { get; set; }
    }

    public enum TextureType : byte
    {
        Texture2D,
        Texture3D,
        CubeMap,
        Array,
    }
}