using Adjutant.Blam.Halo4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Tests.Blam.Halo4
{
    [TestClass]
    public class CacheFile
    {
        private const string BetaFolder = @"Y:\Halo\Halo4Beta";
        private const string RetailSPFolder = @"Y:\Halo\Halo4Retail\Campaign\maps";
        private const string RetailMPFolder = @"Y:\Halo\Halo4Retail\Multiplayer\maps";
        private const string DLCFolder = @"Y:\Halo\Halo4Retail\DLC";

        [DataRow("mainmenu")]
        [DataRow("ca_gore_valley")]
        [DataRow("ca_warhouse")]
        [DataRow("wraparound")]
        [DataTestMethod]
        public void Halo4Beta(string map)
        {
            TestMap(BetaFolder, map);
        }

        [DataRow("mainmenu")]
        [DataRow("m05_prologue")]
        [DataRow("m10_crash")]
        [DataRow("m020")]
        [DataRow("m30_cryptum")]
        [DataRow("m40_invasion")]
        [DataRow("m60_rescue")]
        [DataRow("m70_liftoff")]
        [DataRow("m80_delta")]
        [DataRow("m90_sacrifice")]
        [DataRow("m95_epilogue")]
        [DataRow("onyx_patch")]
        [DataTestMethod]
        public void Halo4Campaign(string map)
        {
            TestMap(RetailSPFolder, map);
        }

        [DataRow("mainmenu")]
        [DataRow("ca_blood_cavern")]
        [DataRow("ca_blood_crash")]
        [DataRow("ca_canyon")]
        [DataRow("ca_forge_bonanza")]
        [DataRow("ca_forge_erosion")]
        [DataRow("ca_forge_ravine")]
        [DataRow("ca_gore_valley")]
        [DataRow("ca_redoubt")]
        [DataRow("ca_tower")]
        [DataRow("ca_warhouse")]
        [DataRow("ff87_chopperbowl")]
        [DataRow("wraparound")]
        [DataRow("z05_cliffside")]
        [DataRow("z11_valhalla")]
        [DataTestMethod]
        public void Halo4Multiplayer(string map)
        {
            TestMap(RetailMPFolder, map);
        }

        [DataRow("dlc_forge_island")]
        [DataRow("Castle\\ca_basin")]
        [DataRow("Castle\\ca_highrise")]
        [DataRow("Castle\\ca_spiderweb")]
        [DataRow("Crimson\\dlc_dejewel")]
        [DataRow("Crimson\\dlc_dejunkyard")]
        [DataRow("Crimson\\zd_02_grind")]
        [DataTestMethod]
        public void Halo4DLC(string map)
        {
            TestMap(DLCFolder, map);
        }

        private void TestMap(string folder, string map)
        {
            var cache = new Adjutant.Blam.Halo4.CacheFile(Path.Combine(folder, $"{map}.map"));

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
