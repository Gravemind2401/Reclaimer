using Adjutant.Blam.Common;
using Adjutant.Blam.Halo3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Tests.Blam.MccHalo3
{
    [TestClass]
    public class CacheFile
    {
        private const string MapsFolder = @"Y:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection\halo3\maps";

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
        public void MccHalo3Campaign(string map)
        {
            TestMap(MapsFolder, map);
        }

        [DataRow("armory")]
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
        public void MccHalo3Multiplayer(string map)
        {
            TestMap(MapsFolder, map);
        }

        private void TestMap(string folder, string map)
        {
            var cache = new Adjutant.Blam.MccHalo3.CacheFile(Path.Combine(folder, $"{map}.map"));

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
