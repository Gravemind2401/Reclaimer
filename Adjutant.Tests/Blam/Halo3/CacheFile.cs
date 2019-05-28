using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Adjutant.IO;
using Adjutant.Blam.Halo3;
using System.Threading.Tasks;

namespace Adjutant.Tests.Blam.Halo3
{
    [TestClass]
    public class CacheFile
    {
        private const string BetaFolder = @"Y:\Halo\Halo3Beta\maps";
        private const string RetailSPFolder = @"Y:\Halo\Halo3Retail\Campaign\maps";
        private const string RetailMPFolder = @"Y:\Halo\Halo3Retail\Multiplayer\maps";
        private const string OdstFolder = @"Y:\Halo\Halo3ODST\maps";

        [DataRow("mainmenu")]
        [DataRow("deadlock")]
        [DataRow("riverworld")]
        [DataRow("snowbound")]
        [DataTestMethod]
        public void Halo3Beta(string map)
        {
            TestMap(BetaFolder, map);
        }

        [DataRow("mainmenu")]
        [DataRow("005_intro")]
        [DataRow("010_jungle")]
        [DataRow("020_base")]
        [DataRow("030_outskirts")]
        [DataRow("040_voi")]
        [DataRow("050_floodvoi")]
        [DataRow("070_waste")]
        [DataRow("100_citadel")]
        [DataRow("110_hc")]
        [DataRow("120_halo")]
        [DataRow("130_epilogue")]
        [DataTestMethod]
        public void Halo3Campaign(string map)
        {
            TestMap(RetailSPFolder, map);
        }

        [DataRow("chill")]
        [DataRow("construct")]
        [DataRow("cyberdyne")]
        [DataRow("deadlock")]
        [DataRow("guardian")]
        [DataRow("isolation")]
        [DataRow("riverworld")]
        [DataRow("salvation")]
        [DataRow("shrine")]
        [DataRow("snowbound")]
        [DataRow("zanzibar")]
        [DataTestMethod]
        public void Halo3Multiplayer(string map)
        {
            TestMap(RetailSPFolder, map);
        }

        [DataRow("mainmenu")]
        [DataRow("armory")]
        [DataRow("armory2")]
        [DataRow("bunkerworld")]
        [DataRow("chill")]
        [DataRow("chillout")]
        [DataRow("construct")]
        [DataRow("cyberdyne")]
        [DataRow("deadlock")]
        [DataRow("descent")]
        [DataRow("docks")]
        [DataRow("fortress")]
        [DataRow("ghosttown")]
        [DataRow("guardian")]
        [DataRow("isolation")]
        [DataRow("lockout")]
        [DataRow("midship")]
        [DataRow("riverworld")]
        [DataRow("salvation")]
        [DataRow("sandbox")]
        [DataRow("shrine")]
        [DataRow("sidewinder")]
        [DataRow("snowbound")]
        [DataRow("spacecamp")]
        [DataRow("warehouse")]
        [DataRow("zanzibar")]
        [DataTestMethod]
        public void Halo3MapPack(string map)
        {
            TestMap(RetailMPFolder, map);
        }

        [DataRow("mainmenu")]
        [DataRow("c100")]
        [DataRow("c200")]
        [DataRow("h100")]
        [DataRow("l200")]
        [DataRow("l300")]
        [DataRow("sc100")]
        [DataRow("sc110")]
        [DataRow("sc120")]
        [DataRow("sc130")]
        [DataRow("sc140")]
        [DataRow("sc150")]
        [DataTestMethod]
        public void Halo3Odst(string map)
        {
            TestMap(OdstFolder, map);
        }

        private void TestMap(string folder, string map)
        {
            var cache = new Adjutant.Blam.Halo3.CacheFile(Path.Combine(folder, $"{map}.map"));

            var t0 = Task.Run(() =>
            {
                var gestalt = cache.TagIndex.FirstOrDefault(t => t.ClassCode == "zone")?.ReadMetadata<cache_file_resource_gestalt>();
                var layoutTable = cache.TagIndex.FirstOrDefault(t => t.ClassCode == "play")?.ReadMetadata<cache_file_resource_layout_table>();

                return true;
            });

            var t1 = Task.Run(() =>
            {
                var bitmaps = cache.TagIndex.Where(i => i.ClassCode == "bitm")
                    .Select(i => i.ReadMetadata<bitmap>())
                    .ToList();

                return true;
            });

            var t2 = Task.Run(() =>
            {
                var models = cache.TagIndex.Where(i => i.ClassCode == "mode")
                .Select(i => i.ReadMetadata<render_model>())
                .ToList();

                return true;
            });

            var t3 = Task.Run(() =>
            {
                var bsps = cache.TagIndex.Where(i => i.ClassCode == "sbsp")
                .Select(i => i.ReadMetadata<scenario_structure_bsp>())
                .ToList();

                return true;
            });

            Assert.IsTrue(t0.GetAwaiter().GetResult());
            Assert.IsTrue(t1.GetAwaiter().GetResult());
            Assert.IsTrue(t2.GetAwaiter().GetResult());
            Assert.IsTrue(t3.GetAwaiter().GetResult());
        }
    }
}
