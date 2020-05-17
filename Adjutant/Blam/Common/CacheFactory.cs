using Adjutant.Utilities;
using System;
using System.Collections.Generic;
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

        internal static readonly string[] SystemClasses = new[] { "scnr", "ugh!", "play", "zone" };

        private static Dictionary<string, string> ReadClassXml(string xml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            return doc.FirstChild.ChildNodes.Cast<XmlNode>()
                .ToDictionary(n => n.Attributes["code"].Value, n => n.Attributes["name"].Value);
        }

        public static ICacheFile ReadCacheFile(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            if (!File.Exists(fileName))
                throw Exceptions.FileNotFound(fileName);

            var detail = CacheDetail.FromFile(fileName);
            switch (detail.CacheType)
            {
                case CacheType.Halo1Xbox:
                case CacheType.Halo1PC:
                case CacheType.Halo1CE:
                case CacheType.Halo1AE:
                    return new Halo1.CacheFile(detail);

                case CacheType.Halo2Xbox:
                case CacheType.Halo2Vista:
                    return new Halo2.CacheFile(detail);

                case CacheType.Halo3Beta:
                case CacheType.Halo3Retail:
                case CacheType.Halo3ODST:
                    return new Halo3.CacheFile(detail);

                case CacheType.HaloReachBeta:
                case CacheType.HaloReachRetail:
                    return new HaloReach.CacheFile(detail);

                case CacheType.Halo4Beta:
                case CacheType.Halo4Retail:
                    return new Halo4.CacheFile(detail);

                case CacheType.MccHaloReach:
                    return new MccHaloReach.CacheFile(detail);

                default: throw Exceptions.NotAValidMapFile(fileName);
            }
        }

        public static int GetCacheGeneration(this CacheType cacheType)
        {
            var field = typeof(CacheType).GetField(cacheType.ToString());
            return field.GetCustomAttributes(typeof(CacheGenerationAttribute), false).OfType<CacheGenerationAttribute>().FirstOrDefault()?.Generation ?? -1;
        }

        public static DependencyReader CreateReader(this ICacheFile cache, IAddressTranslator translator)
        {
            var fs = new FileStream(cache.FileName, FileMode.Open, FileAccess.Read);
            var reader = new DependencyReader(fs, cache.ByteOrder);

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
    }
}
