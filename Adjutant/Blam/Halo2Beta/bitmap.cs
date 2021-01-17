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

        private static readonly Dictionary<TextureFormat, DxgiFormat> dxgiLookup = new Dictionary<TextureFormat, DxgiFormat>
        {
            { TextureFormat.DXT1, DxgiFormat.BC1_UNorm },
            { TextureFormat.DXT3, DxgiFormat.BC2_UNorm },
            { TextureFormat.DXT5, DxgiFormat.BC3_UNorm },
            { TextureFormat.A8R8G8B8, DxgiFormat.B8G8R8A8_UNorm },
            { TextureFormat.X8R8G8B8, DxgiFormat.B8G8R8X8_UNorm },
            { TextureFormat.R5G6B5, DxgiFormat.B5G6R5_UNorm },
            { TextureFormat.A1R5G5B5, DxgiFormat.B5G5R5A1_UNorm },
            { TextureFormat.A4R4G4B4, DxgiFormat.B4G4R4A4_UNorm }
        };

        private static readonly Dictionary<TextureFormat, XboxFormat> xboxLookup = new Dictionary<TextureFormat, XboxFormat>
        {
            { TextureFormat.A8, XboxFormat.A8 },
            { TextureFormat.A8Y8, XboxFormat.Y8A8 },
            { TextureFormat.AY8, XboxFormat.AY8 },
            { TextureFormat.P8, XboxFormat.Y8 },
            { TextureFormat.P8_bump, XboxFormat.Y8 },
            { TextureFormat.Y8, XboxFormat.Y8 }
        };

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

            DdsImage dds;
            if (dxgiLookup.ContainsKey(submap.BitmapFormat))
                dds = new DdsImage(submap.Height, submap.Width, dxgiLookup[submap.BitmapFormat], DxgiTextureType.Texture2D, data);
            else if (xboxLookup.ContainsKey(submap.BitmapFormat))
                dds = new DdsImage(submap.Height, submap.Width, xboxLookup[submap.BitmapFormat], DxgiTextureType.Texture2D, data);
            else throw Exceptions.BitmapFormatNotSupported(submap.BitmapFormat.ToString());

            ////mipmaps are getting in the way
            //if (submap.BitmapType == TextureType.CubeMap)
            //{
            //    dds.TextureFlags = TextureFlags.DdsSurfaceFlagsCubemap;
            //    dds.CubemapFlags = CubemapFlags.DdsCubemapAllFaces;
            //    dds.DX10ResourceFlags = D3D10ResourceMiscFlags.TextureCube;
            //}

            return dds;
        } 

        #endregion
    }
}
