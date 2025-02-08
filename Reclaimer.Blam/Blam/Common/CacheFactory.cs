using Reclaimer.Blam.Utilities;
using Reclaimer.Drawing;
using Reclaimer.IO;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Xml;

namespace Reclaimer.Blam.Common
{
    public static class CacheFactory
    {
        private static Dictionary<string, string> halo1Classes;
        internal static IReadOnlyDictionary<string, string> Halo1Classes => halo1Classes ??= ReadClassXml(Properties.Resources.Halo1Classes);

        private static Dictionary<string, string> halo2Classes;
        internal static IReadOnlyDictionary<string, string> Halo2Classes => halo2Classes ??= ReadClassXml(Properties.Resources.Halo2Classes);

        internal const string ScenarioClass = "scnr";
        internal static readonly string[] SystemClasses = ["scnr", "matg", "ugh!", "play", "zone"];

        internal static Dictionary<string, string> ReadClassXml(string xml)
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
            ArgumentNullException.ThrowIfNull(fileName);
            Exceptions.ThrowIfFileNotFound(fileName);

            var args = CacheArgs.FromFile(fileName);
            var (engine, cacheType, isMcc) = (args.Metadata?.Engine ?? BlamEngine.Unknown, args.Metadata?.CacheType ?? CacheType.Unknown, args.Metadata?.IsMcc ?? false);

            return engine switch
            {
                BlamEngine.Halo1 => new Halo1.CacheFile(args),

                BlamEngine.Halo2 when cacheType <= CacheType.Halo2Beta => new Halo2Beta.CacheFile(args),
                BlamEngine.Halo2 when cacheType == CacheType.MccHalo2 => throw Exceptions.UnknownMapFile(fileName),
                BlamEngine.Halo2 => new Halo2.CacheFile(args),

                BlamEngine.Halo3 when cacheType <= CacheType.Halo3Delta => new Halo3Alpha.CacheFile(args),
                BlamEngine.Halo3 when !isMcc => new Halo3.CacheFile(args),
                BlamEngine.Halo3 when cacheType < CacheType.MccHalo3F6 => new MccHalo3.CacheFile(args),
                BlamEngine.Halo3 => new MccHalo3.CacheFileU6(args),

                BlamEngine.Halo3ODST when !isMcc => new Halo3.CacheFile(args),
                BlamEngine.Halo3ODST when cacheType < CacheType.MccHalo3ODSTF3 => new MccHalo3.CacheFile(args),
                BlamEngine.Halo3ODST => new MccHalo3.CacheFileU6(args),

                BlamEngine.HaloReach when !isMcc => new HaloReach.CacheFile(args),
                BlamEngine.HaloReach when cacheType < CacheType.MccHaloReachU8 => new MccHaloReach.CacheFile(args),
                BlamEngine.HaloReach => new MccHaloReach.CacheFileU8(args),

                BlamEngine.Halo4 when !isMcc => new Halo4.CacheFile(args),
                BlamEngine.Halo4 when cacheType < CacheType.MccHalo4U4 => new MccHalo4.CacheFile(args),
                BlamEngine.Halo4 => new MccHalo4.CacheFileU4(args),

                BlamEngine.Halo2X when cacheType < CacheType.MccHalo2XU8 => new MccHalo2X.CacheFile(args),
                BlamEngine.Halo2X => new MccHalo2X.CacheFileU8(args),

                _ => throw Exceptions.UnknownMapFile(fileName)
            };
        }

        public static int GetHeaderSize(this Gen3.IGen3CacheFile cache) => (int)FixedSizeAttribute.ValueFor(cache.Header.GetType(), (int)cache.CacheType);

        public static Stream CreateStream(this ICacheFile cache)
        {
            var stream = (Stream)new FileStream(cache.FileName, FileMode.Open, FileAccess.Read);
            stream = cache switch
            {
                Halo1.CacheFile h1cache when h1cache.CacheType == CacheType.Halo1Xbox && h1cache.IsCompressed => new Halo1.XboxCompressedCacheStream(h1cache, stream),
                Halo2.CacheFile h2cache when h2cache.IsCompressed => new Halo2.MccCompressedCacheStream(h2cache, stream),
                _ => stream
            };

            return stream;
        }

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator) => CreateReader(cache, translator, false);

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator, bool leaveOpen)
        {
            var stream = CreateStream(cache);
            return CreateReader(cache, translator, stream, leaveOpen);
        }

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator, Stream stream) => CreateReader(cache, translator, stream, false);

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator, Stream stream, bool leaveOpen)
        {
            var reader = new DependencyReader(stream, cache.ByteOrder, leaveOpen);

            reader.RegisterInstance(cache);
            reader.RegisterInstance(translator);

            if (cache.CacheType >= CacheType.Halo2Xbox)
                reader.RegisterType(reader.ReadMatrix3x4);

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
