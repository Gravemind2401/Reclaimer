using Reclaimer.Blam.Halo1;

namespace Reclaimer.Blam.Tests.Halo1
{
    [TestClass]
    public class CacheFile
    {
        [DataRow("a10")]
        [DataRow("a30")]
        [DataRow("b30")]
        [DataRow("b40")]
        [DataRow("c10")]
        [DataRow("c20")]
        [DataRow("c40")]
        [DataRow("d20")]
        [DataRow("d40")]
        [DataTestMethod]
        public void Halo1Campaign(string map) => TestMap(map);

        [DataRow("beavercreek")]
        [DataRow("bloodgulch")]
        [DataRow("boardingaction")]
        [DataRow("carousel")]
        [DataRow("chillout")]
        [DataRow("damnation")]
        [DataRow("deathisland")]
        [DataRow("gephyrophobia")]
        [DataRow("hangemhigh")]
        [DataRow("icefields")]
        [DataRow("infinity")]
        [DataRow("longest")]
        [DataRow("prisoner")]
        [DataRow("putput")]
        [DataRow("ratrace")]
        [DataRow("sidewinder")]
        [DataRow("timberland")]
        [DataRow("wizard")]
        [DataTestMethod]
        public void Halo1Multiplayer(string map) => TestMap(map);

        private static void TestMap(string map)
        {
            var cache = new Blam.Halo1.CacheFile(Path.Combine(Directories.PCHalo1, $"{map}.map"));

            var t1 = Task.Run(() =>
            {
                var bitmaps = cache.TagIndex.Where(i => i.ClassCode == "bitm")
                    .Select(i => i.ReadMetadata<BitmapTag>())
                    .ToList();

                return true;
            });

            var t2 = Task.Run(() =>
            {
                var models = cache.TagIndex.Where(i => i.ClassCode == "mod2")
                .Select(i => i.ReadMetadata<GbxModelTag>())
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

            Assert.IsTrue(t1.GetAwaiter().GetResult());
            Assert.IsTrue(t2.GetAwaiter().GetResult());
            Assert.IsTrue(t3.GetAwaiter().GetResult());
        }
    }
}
