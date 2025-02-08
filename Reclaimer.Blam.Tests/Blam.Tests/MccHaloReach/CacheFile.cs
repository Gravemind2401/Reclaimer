using Reclaimer.Blam.HaloReach;

namespace Reclaimer.Blam.Tests.MccHaloReach
{
    [TestClass]
    public class CacheFile
    {
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
        public void MccHaloReachCampaign(string map) => TestMap(Directories.MccHaloReach, map);

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
        public void MccHaloReachMultiplayer(string map) => TestMap(Directories.MccHaloReach, map);

        [DataRow("ff10_prototype")]
        [DataRow("ff20_courtyard")]
        [DataRow("ff30_waterfront")]
        [DataRow("ff45_corvette")]
        [DataRow("ff50_park")]
        [DataRow("ff60_airview")]
        [DataRow("ff60_icecave")]
        [DataRow("ff70_holdout")]
        [DataTestMethod]
        public void MccHaloReachFirefight(string map) => TestMap(Directories.MccHaloReach, map);

        [DataRow("cex_beaver_creek")]
        [DataRow("cex_damnation")]
        [DataRow("cex_ff_halo")]
        [DataRow("cex_hangemhigh")]
        [DataRow("cex_headlong")]
        [DataRow("cex_prisoner")]
        [DataRow("cex_timberland")]
        [DataTestMethod]
        public void HaloReachCex(string map) => TestMap(Directories.MccHaloReach, map);

        private void TestMap(string folder, string map)
        {
            var cache = new Blam.MccHaloReach.CacheFileU8(Path.Combine(folder, $"{map}.map"));

            var t0 = Task.Run(() =>
            {
                var gestalt = cache.TagIndex.FirstOrDefault(t => t.ClassCode == "zone")?.ReadMetadata<CacheFileResourceGestaltTag>();
                var layoutTable = cache.TagIndex.FirstOrDefault(t => t.ClassCode == "play")?.ReadMetadata<CacheFileResourceLayoutTableTag>();

                return true;
            });

            var t1 = Task.Run(() =>
            {
                var bitmaps = cache.TagIndex.Where(i => i.ClassCode == "bitm")
                    .Select(i => i.ReadMetadata<BitmapTag>())
                    .ToList();

                return true;
            });

            var t2 = Task.Run(() =>
            {
                var models = cache.TagIndex.Where(i => i.ClassCode == "mode")
                .Select(i => i.ReadMetadata<RenderModelTag>())
                .ToList();

                return true;
            });

            var t3 = Task.Run(() =>
            {
                var bsps = cache.TagIndex.Where(i => i.ClassCode == "sbsp")
                .Select(i => i.ReadMetadata<ScenarioStructureBspTag>())
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
