using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reclaimer.Blam.HaloReach;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Tests.Blam.MccHaloReach
{
    [TestClass]
    public class CacheFile
    {
        private const string MapsFolder = @"Y:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection\haloreach\maps";

        [DataRow("m05")]
        [DataRow("m10")]
        [DataRow("m20")]
        [DataRow("m30")]
        [DataRow("m35")]
        [DataRow("m45")]
        [DataRow("m50")]
        [DataRow("m52")]
        [DataRow("m60")]
        [DataRow("m70")]
        [DataRow("m70_a")]
        [DataRow("m70_bonus")]
        [DataTestMethod]
        public void MccHaloReachCampaign(string map)
        {
            TestMap(MapsFolder, map);
        }

        [DataRow("20_sword_slayer")]
        [DataRow("30_settlement")]
        [DataRow("35_island")]
        [DataRow("45_aftship")]
        [DataRow("45_launch_station")]
        [DataRow("50_panopticon")]
        [DataRow("52_ivory_tower")]
        [DataRow("70_boneyard")]
        [DataRow("forge_halo")]
        [DataTestMethod]
        public void MccHaloReachMultiplayer(string map)
        {
            TestMap(MapsFolder, map);
        }

        [DataRow("ff10_prototype")]
        [DataRow("ff20_courtyard")]
        [DataRow("ff30_waterfront")]
        [DataRow("ff45_corvette")]
        [DataRow("ff50_park")]
        [DataRow("ff60_airview")]
        [DataRow("ff60_icecave")]
        [DataRow("ff70_holdout")]
        [DataTestMethod]
        public void MccHaloReachFirefight(string map)
        {
            TestMap(MapsFolder, map);
        }

        [DataRow("cex_beaver_creek")]
        [DataRow("cex_damnation")]
        [DataRow("cex_ff_halo")]
        [DataRow("cex_hangemhigh")]
        [DataRow("cex_headlong")]
        [DataRow("cex_prisoner")]
        [DataRow("cex_timberland")]
        [DataTestMethod]
        public void HaloReachCex(string map)
        {
            TestMap(MapsFolder, map);
        }

        private void TestMap(string folder, string map)
        {
            var cache = new Reclaimer.Blam.MccHaloReach.CacheFile(Path.Combine(folder, $"{map}.map"));

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
