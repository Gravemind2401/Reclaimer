using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Dds;

namespace Adjutant.Saber3D.Halo1X
{
    public class Texture : IBitmap
    {
        private const int LittleHeader = 0x50494354; //TCIP
        private const int BigHeader = 0x54434950; //PICT

        private readonly PakItem item;
        private readonly bool isBigEndian;

        public int Width { get; }
        public int Height { get; }
        public int MapCount { get; }
        public TextureFormat Format { get; }
        public int DataOffset { get; }

        #region IBitmap

        private static readonly Dictionary<TextureFormat, DxgiFormat> dxgiLookup = new Dictionary<TextureFormat, DxgiFormat>
        {
            { TextureFormat.DXT1, DxgiFormat.BC1_UNorm },
            { TextureFormat.DXT3, DxgiFormat.BC2_UNorm },
            { TextureFormat.DXT5, DxgiFormat.BC3_UNorm },
            { TextureFormat.A8R8G8B8, DxgiFormat.B8G8R8A8_UNorm },
            { TextureFormat.X8R8G8B8, DxgiFormat.B8G8R8X8_UNorm }
        };

        private static readonly Dictionary<TextureFormat, XboxFormat> xboxLookup = new Dictionary<TextureFormat, XboxFormat>
        {
            { TextureFormat.A8Y8, XboxFormat.Y8A8 },
            { TextureFormat.DXT5a, XboxFormat.DXT5a_scalar },
            { TextureFormat.DXN, XboxFormat.DXN }
        };

        string IBitmap.SourceFile => item.Container.FileName;

        int IBitmap.Id => item.Address;

        string IBitmap.Name => item.Name;

        string IBitmap.Class => item.ItemType.ToString();

        int IBitmap.SubmapCount => 1;

        CubemapLayout IBitmap.CubeLayout => new CubemapLayout
        {

        };

        DdsImage IBitmap.ToDds(int index)
        {
            if (index < 0 || index >= 1)
                throw new ArgumentOutOfRangeException(nameof(index));

            byte[] data;
            using (var reader = item.Container.CreateReader())
            {
                var size = Height * Width * MapCount * Format.Bpp();
                reader.Seek(item.Address + DataOffset, SeekOrigin.Begin);
                data = reader.ReadBytes(size);
            }

            if (isBigEndian)
            {
                var bpp = Format.Bpp();
                if (bpp > 1)
                {
                    for (int i = 0; i < data.Length; i += bpp)
                        Array.Reverse(data, i, bpp);
                }
            }

            DdsImage dds;
            if (dxgiLookup.ContainsKey(Format))
                dds = new DdsImage(Height * MapCount, Width, dxgiLookup[Format], DxgiTextureType.Texture2D, data);
            else if (xboxLookup.ContainsKey(Format))
                dds = new DdsImage(Height * MapCount, Width, xboxLookup[Format], DxgiTextureType.Texture2D, data);
            else throw Exceptions.BitmapFormatNotSupported(Format.ToString());

            //if (MapCount == 6)
            //{
            //    dds.TextureFlags = TextureFlags.DdsSurfaceFlagsCubemap;
            //    dds.CubemapFlags = CubemapFlags.DdsCubemapAllFaces;
            //    dds.DX10ResourceFlags = D3D10ResourceMiscFlags.TextureCube;
            //}

            return dds;
        }
        #endregion

        public Texture(PakItem item)
        {
            this.item = item;

            using (var x = item.Container.CreateReader())
            using (var reader = x.CreateVirtualReader(item.Address))
            {
                reader.Seek(6, SeekOrigin.Begin);
                var head = reader.ReadInt32();
                if (head == LittleHeader)
                    reader.ByteOrder = ByteOrder.LittleEndian;
                else
                {
                    reader.Seek(8, SeekOrigin.Begin);
                    head = reader.ReadInt32();

                    if (head == BigHeader)
                        reader.ByteOrder = ByteOrder.BigEndian;
                    else throw Exceptions.NotASaberTextureItem(item);

                    isBigEndian = true;
                }

                reader.Seek(isBigEndian ? 12 : 16, SeekOrigin.Begin);
                Width = reader.ReadInt32();
                Height = reader.ReadInt32();

                reader.Seek(isBigEndian ? 24 : 28, SeekOrigin.Begin);
                MapCount = reader.ReadInt32();

                reader.Seek(isBigEndian ? 32 : 38, SeekOrigin.Begin);
                Format = (TextureFormat)reader.ReadInt32();
                if (Format == TextureFormat.AlsoDXT1)
                    Format = TextureFormat.DXT1; //for compatibility with KnownTextureFormat

                DataOffset = isBigEndian ? 4096 : 58;
            }
        }
    }

    public enum TextureFormat
    {
        A8R8G8B8 = 0,
        A8Y8 = 10,
        DXT1 = 12,
        AlsoDXT1 = 13,
        DXT3 = 15,
        DXT5 = 17,
        X8R8G8B8 = 22,
        DXN = 36,
        DXT5a = 37
    }
}
