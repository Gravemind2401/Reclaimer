using Adjutant.Blam.Common;
using Adjutant.Blam.Halo2;
using Adjutant.Spatial;
using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Dds;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Blam.Halo2Beta
{
    public class bitmap : IBitmap
    {
        private readonly ICacheFile cache;
        private readonly IIndexItem item;

        public bitmap(ICacheFile cache, IIndexItem item)
        {
            this.cache = cache;
            this.item = item;
        }

        [Offset(96)]
        public BlockCollection<BitmapDataBlock> Bitmaps { get; set; }

        #region IBitmap

        private static readonly CubemapLayout Halo2CubeLayout = CubemapLayout.NonCubemap;

        string IBitmap.SourceFile => item.CacheFile.FileName;

        int IBitmap.Id => item.Id;

        string IBitmap.Name => item.FullPath;

        string IBitmap.Class => item.ClassName;

        int IBitmap.SubmapCount => Bitmaps.Count;

        CubemapLayout IBitmap.CubeLayout => Halo2CubeLayout;

        public DdsImage ToDds(int index)
        {
            if (index < 0 || index >= Bitmaps.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var submap = Bitmaps[index];
            var data = submap.Lod0Pointer.ReadData(submap.Lod0Size);

            if (submap.Flags.HasFlag(BitmapFlags.Swizzled))
            {
                var bpp = submap.BitmapFormat.Bpp();
                data = TextureUtils.Swizzle(data, submap.Width, submap.Height, 1, bpp);
            }

            return TextureUtils.GetDds(submap.Height, submap.Width, submap.BitmapFormat, false, data);

            ////mipmaps are getting in the way
            //if (submap.BitmapType == TextureType.CubeMap)
            //{
            //    dds.TextureFlags = TextureFlags.DdsSurfaceFlagsCubemap;
            //    dds.CubemapFlags = CubemapFlags.DdsCubemapAllFaces;
            //    dds.DX10ResourceFlags = D3D10ResourceMiscFlags.TextureCube;
            //}
        } 

        #endregion
    }
}
