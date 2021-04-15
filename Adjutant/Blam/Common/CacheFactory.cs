using Adjutant.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Dds;
using System.IO;
using System.IO.Endian;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Adjutant.Blam.Common
{
    public static class CacheFactory
    {
        private static Dictionary<string, string> halo1Classes;
        internal static IReadOnlyDictionary<string, string> Halo1Classes
        {
            get
            {
                if (halo1Classes == null)
                    halo1Classes = ReadClassXml(Properties.Resources.Halo1Classes);

                return halo1Classes;
            }
        }

        private static Dictionary<string, string> halo2Classes;
        internal static IReadOnlyDictionary<string, string> Halo2Classes
        {
            get
            {
                if (halo2Classes == null)
                    halo2Classes = ReadClassXml(Properties.Resources.Halo2Classes);

                return halo2Classes;
            }
        }

        internal const string ScenarioClass = "scnr";
        internal static readonly string[] SystemClasses = new[] { "scnr", "matg", "ugh!", "play", "zone" };

        private static Dictionary<string, string> ReadClassXml(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc.FirstChild.ChildNodes.Cast<XmlNode>()
                .ToDictionary(n => n.Attributes["code"].Value, n => n.Attributes["name"].Value);
        }

        internal static readonly CubemapLayout Gen3CubeLayout = new CubemapLayout
        {
            Face1 = CubemapFace.Right,
            Face2 = CubemapFace.Left,
            Face3 = CubemapFace.Back,
            Face4 = CubemapFace.Front,
            Face5 = CubemapFace.Top,
            Face6 = CubemapFace.Bottom,
            Orientation1 = RotateFlipType.Rotate270FlipNone,
            Orientation2 = RotateFlipType.Rotate90FlipNone,
            Orientation3 = RotateFlipType.Rotate180FlipNone,
            Orientation6 = RotateFlipType.Rotate180FlipNone
        };

        internal static readonly CubemapLayout MccGen3CubeLayout = new CubemapLayout
        {
            Face1 = CubemapFace.Right,
            Face2 = CubemapFace.Back,
            Face3 = CubemapFace.Left,
            Face4 = CubemapFace.Front,
            Face5 = CubemapFace.Top,
            Face6 = CubemapFace.Bottom,
            Orientation1 = RotateFlipType.Rotate270FlipNone,
            Orientation2 = RotateFlipType.Rotate180FlipNone,
            Orientation3 = RotateFlipType.Rotate90FlipNone,
            Orientation6 = RotateFlipType.Rotate180FlipNone
        };

        public static ICacheFile ReadCacheFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            var args = CacheArgs.FromFile(fileName);
            switch (args.Metadata?.CacheType)
            {
                case CacheType.Halo1Xbox:
                case CacheType.Halo1PC:
                case CacheType.Halo1CE:
                case CacheType.Halo1AE:
                    return new Halo1.CacheFile(args);

                case CacheType.Halo2Beta:
                    return new Halo2Beta.CacheFile(args);

                case CacheType.Halo2Xbox:
                case CacheType.Halo2Vista:
                    return new Halo2.CacheFile(args);

                case CacheType.Halo3Alpha:
                    return new Halo3Alpha.CacheFile(args);

                case CacheType.Halo3Beta:
                case CacheType.Halo3Retail:
                case CacheType.Halo3ODST:
                    return new Halo3.CacheFile(args);

                case CacheType.MccHalo3:
                case CacheType.MccHalo3U4:
                case CacheType.MccHalo3ODST:
                    return new MccHalo3.CacheFile(args);

                case CacheType.HaloReachBeta:
                case CacheType.HaloReachRetail:
                    return new HaloReach.CacheFile(args);

                case CacheType.MccHaloReach:
                case CacheType.MccHaloReachU3:
                    return new MccHaloReach.CacheFile(args);

                case CacheType.Halo4Beta:
                case CacheType.Halo4Retail:
                    return new Halo4.CacheFile(args);

                case CacheType.MccHalo4:
                    return new MccHalo4.CacheFile(args);

                case CacheType.MccHalo2X:
                    return new MccHalo2X.CacheFile(args);

                default: throw Exceptions.UnknownMapFile(fileName);
            }
        }

        public static int GetHeaderSize(this Gen3.IGen3CacheFile cache)
        {
            return (int)FixedSizeAttribute.ValueFor(cache.Header.GetType(), (int)cache.CacheType);
        }

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator) => CreateReader(cache, translator, false);

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator, bool leaveOpen)
        {
            var fs = new FileStream(cache.FileName, FileMode.Open, FileAccess.Read);
            return CreateReader(cache, translator, fs, leaveOpen);
        }

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator, Stream stream) => CreateReader(cache, translator, stream, false);

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator, Stream stream, bool leaveOpen)
        {
            var reader = new DependencyReader(stream, cache.ByteOrder, leaveOpen);

            reader.RegisterInstance(cache);
            reader.RegisterInstance(translator);

            if (cache.CacheType >= CacheType.Halo2Xbox)
            {
                reader.RegisterType(() => new Matrix4x4
                {
                    M11 = reader.ReadSingle(),
                    M12 = reader.ReadSingle(),
                    M13 = reader.ReadSingle(),

                    M21 = reader.ReadSingle(),
                    M22 = reader.ReadSingle(),
                    M23 = reader.ReadSingle(),

                    M31 = reader.ReadSingle(),
                    M32 = reader.ReadSingle(),
                    M33 = reader.ReadSingle(),

                    M41 = reader.ReadSingle(),
                    M42 = reader.ReadSingle(),
                    M43 = reader.ReadSingle(),
                });
            }

            return reader;
        }

        public static EndianWriterEx CreateWriter(this ICacheFile cache)
        {
            var fs = new FileStream(cache.FileName, FileMode.Open, FileAccess.ReadWrite);
            return CreateWriter(cache, fs, false);
        }

        public static EndianWriterEx CreateWriter(this ICacheFile cache, Stream stream) => CreateWriter(cache, stream, false);

        public static EndianWriterEx CreateWriter(this ICacheFile cache, Stream stream, bool leaveOpen)
        {
            var writer = new EndianWriterEx(stream, cache.ByteOrder, leaveOpen);

            if (cache.CacheType >= CacheType.Halo2Xbox)
            {
                writer.RegisterType<Matrix4x4>((m, v) =>
                {
                    writer.Write(m.M11);
                    writer.Write(m.M12);
                    writer.Write(m.M13);

                    writer.Write(m.M21);
                    writer.Write(m.M22);
                    writer.Write(m.M23);

                    writer.Write(m.M31);
                    writer.Write(m.M32);
                    writer.Write(m.M33);

                    writer.Write(m.M41);
                    writer.Write(m.M42);
                    writer.Write(m.M43);
                });
            }

            return writer;
        }
    }
}
