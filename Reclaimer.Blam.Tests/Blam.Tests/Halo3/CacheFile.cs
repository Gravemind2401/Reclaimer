using Reclaimer.Blam.Halo3;

namespace Reclaimer.Blam.Tests.Halo3
{
    [TestClass]
    public class CacheFile
    {
        [DataRow("mainmenu")]
        [DataRow("deadlock")]
        [DataRow("riverworld")]
        [DataRow("snowbound")]
        [DataTestMethod]
        public void Halo3Beta(string map) => TestMap(Directories.ConsoleHalo3Beta, map);

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
        public void Halo3Campaign(string map) => TestMap(Directories.ConsoleHalo3, map);

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
        public void Halo3Multiplayer(string map) => TestMap(Directories.ConsoleHalo3Multiplayer, map);

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
        public void Halo3MapPack(string map) => TestMap(Directories.ConsoleHalo3Multiplayer, map);

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
        public void Halo3Odst(string map) => TestMap(Directories.ConsoleHalo3ODST, map);

        private void TestMap(string folder, string map)
        {
            var cache = new Blam.Halo3.CacheFile(Path.Combine(folder, $"{map}.map"));

            var t0 = Task.Run(() =>
            {
                var gestalt = cache.TagIndex.FirstOrDefault(t => t.ClassCode == "zone")?.ReadMetadata<CacheFileResourceGestaltTag>();
                var layoutTable = cache.TagIndex.FirstOrDefault(t => t.ClassCode == "play")?.ReadMetadata<CacheFileResourceLayoutTableTag>();
                var soundGestalt = cache.TagIndex.FirstOrDefault(t => t.ClassCode == "ugh!")?.ReadMetadata<SoundCacheFileGestaltTag>();

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

            var t4 = Task.Run(() =>
            {
                var bsps = cache.TagIndex.Where(i => i.ClassCode == "snd!")
                .Select(i => i.ReadMetadata<SoundTag>())
                .ToList();

                return true;
            });

            Assert.IsTrue(t0.GetAwaiter().GetResult());
            Assert.IsTrue(t1.GetAwaiter().GetResult());
            Assert.IsTrue(t2.GetAwaiter().GetResult());
            Assert.IsTrue(t3.GetAwaiter().GetResult());
            Assert.IsTrue(t4.GetAwaiter().GetResult());
        }
    }
}
