﻿using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.IO;
using Reclaimer.Saber3D.Common;
using Reclaimer.Utilities;
using System.IO;

namespace Reclaimer.Saber3D.Halo1X
{
    public class Texture : ContentItemDefinition, IContentProvider<IBitmap>, IBitmap
    {
        private const int LittleHeader = 0x50494354; //TCIP
        private const int BigHeader = 0x54434950; //PICT

        private readonly bool isBigEndian;

        public Texture(IPakItem item)
            : base(item)
        {
            if (item is InplacePakItem inplace)
            {
                Width = inplace.Width;
                Height = inplace.Height;
                MapCount = inplace.FaceCount;
                Format = inplace.Format;
                DataOffset = 58;
                return;
            }

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

                    reader.ByteOrder = head == BigHeader
                        ? ByteOrder.BigEndian
                        : throw Exceptions.NotASaberTextureItem(item);

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

        public int Width { get; }
        public int Height { get; }
        public int MapCount { get; }
        public TextureFormat Format { get; }
        public int DataOffset { get; }

        #region IContentProvider

        IBitmap IContentProvider<IBitmap>.GetContent() => this;

        int IBitmap.SubmapCount => 1;

        CubemapLayout IBitmap.CubeLayout => CubemapLayout.NonCubemap;

        DdsImage IBitmap.ToDds(int index)
        {
            Exceptions.ThrowIfIndexOutOfRange(index, 1);

            var props = new BitmapProperties(Width, Height, Format, "Texture2D")
            {
                ByteOrder = isBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian,
                FrameCount = MapCount
            };

            byte[] data;
            using (var reader = Container.CreateReader())
            {
                reader.Seek(Item.Address + DataOffset, SeekOrigin.Begin);
                data = reader.ReadBytes(TextureUtils.GetBitmapDataLength(props, false));
            }

            return TextureUtils.GetDds(props, data, false);
        }
        #endregion
    }

    public enum TextureFormat : int
    {
        A8R8G8B8 = 0,
        A8Y8 = 10,
        DXT1 = 12,
        AlsoDXT1 = 13,
        DXT3 = 15,
        DXT5 = 17,
        X8R8G8B8 = 22,
        DXN = 36,
        DXT5a = 37,

        //MCC formats
        AlsoA8Y8 = 48,
        BC1_unorm = 70,
        BC2_unorm = 73,
        BC3_unorm = 76,
        BC4_unorm = 79,
        AlsoDXN = 82,
        AlsoA8R8G8B8 = 90
    }
}
