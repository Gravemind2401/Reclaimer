using Adjutant.Audio;
using Adjutant.Blam.Common;
using Adjutant.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing.Dds;
using System.IO.Endian;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Utilities
{
    public interface IBitmap
    {
        int Id { get; }
        string Name { get; }
        string Class { get; }
        string SourceFile { get; }
        int SubmapCount { get; }
        CubemapLayout CubeLayout { get; }
        DdsImage ToDds(int index);
    }

    public interface IBitmapData
    {
        ByteOrder ByteOrder { get; }
        bool UsesPadding { get; }
        MipmapLayout CubeMipLayout { get; }
        MipmapLayout ArrayMipLayout { get; }

        int Width { get; }
        int Height { get; }
        int Depth { get; }
        int FrameCount { get; }
        int MipmapCount { get; }

        object BitmapFormat { get; }
        object BitmapType { get; }

        bool Swizzled { get; }
    }

    public interface IRenderGeometry
    {
        string SourceFile { get; }
        int Id { get; }
        string Name { get; }
        string Class { get; }
        int LodCount { get; }
        IGeometryModel ReadGeometry(int lod);
        IEnumerable<IBitmap> GetAllBitmaps();
        IEnumerable<IBitmap> GetBitmaps(IEnumerable<int> shaderIndexes);
    }

    public interface ISoundContainer
    {
        string Name { get; }
        string Class { get; }
        GameSound ReadData();
    }
}
