using Adjutant.Blam.Common;
using Adjutant.Blam.Halo2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Adjutant.Tests.Blam.Halo2
{
    [TestClass]
    public class CacheFile
    {
        private const string MapsFolder = @"Y:\Halo\Halo2Xbox\maps";

        [DataRow("mainmenu")]
        [DataRow("00a_introduction")]
        [DataRow("01a_tutorial")]
        [DataRow("01b_spacestation")]
        [DataRow("03a_oldmombasa")]
        [DataRow("03b_newmombasa")]
        [DataRow("04a_gasgiant")]
        [DataRow("04b_floodlab")]
        [DataRow("05a_deltaapproach")]
        [DataRow("05b_deltatowers")]
        [DataRow("06a_sentinelwalls")]
        [DataRow("06b_floodzone")]
        [DataRow("07a_highcharity")]
        [DataRow("07b_forerunnership")]
        [DataRow("08a_deltacliffs")]
        [DataRow("08b_deltacontrol")]
        [DataTestMethod]
        public void Halo2Campaign(string map)
        {
            TestMap(map);
        }

        [DataRow("ascension")]
        [DataRow("beavercreek")]
        [DataRow("burial_mounds")]
        [DataRow("coagulation")]
        [DataRow("colossus")]
        [DataRow("cyclotron")]
        [DataRow("dune")]
        [DataRow("foundation")]
        [DataRow("headlong")]
        [DataRow("lockout")]
        [DataRow("midship")]
        [DataRow("waterworks")]
        [DataRow("zanzibar")]
        [DataTestMethod]
        public void Halo2Multiplayer(string map)
        {
            TestMap(map);
        }

        private void TestMap(string map)
        {
            var cache = new Adjutant.Blam.Halo2.CacheFile(Path.Combine(MapsFolder, $"{map}.map"));

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

            var t4 = Task.Run(() =>
            {
                var bsps = cache.TagIndex.Where(i => i.ClassCode == "snd!")
                .Select(i => i.ReadMetadata<sound>())
                .ToList();

                return true;
            });

            Assert.IsTrue(t1.GetAwaiter().GetResult());
            Assert.IsTrue(t2.GetAwaiter().GetResult());
            Assert.IsTrue(t3.GetAwaiter().GetResult());
            Assert.IsTrue(t4.GetAwaiter().GetResult());
        }
    }
}
