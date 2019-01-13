using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Adjutant.IO;
using Adjutant.Blam.Halo1;

namespace Adjutant.Tests.Blam.Halo1
{
    [TestClass]
    public class CacheFile
    {
        private const string MapsFolder = @"Y:\Halo\Halo1PC\MAPS";

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
        public void Halo1Campaign(string map)
        {
            var cache = new Adjutant.Blam.Halo1.CacheFile(Path.Combine(MapsFolder, $"{map}.map"));

            var models = cache.Index.Where(i => i.ClassCode == "mod2")
                .Select(i => i.ReadMetadata<gbxmodel>())
                .ToList();

            var bsps = cache.Index.Where(i => i.ClassCode == "sbsp")
                .Select(i => i.ReadMetadata<scenario_structure_bsp>())
                .ToList();
        }

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
        public void Halo1Multiplayer(string map)
        {
            var cache = new Adjutant.Blam.Halo1.CacheFile(Path.Combine(MapsFolder, $"{map}.map"));

            var models = cache.Index.Where(i => i.ClassCode == "mod2")
                .Select(i => i.ReadMetadata<gbxmodel>())
                .ToList();

            var bsps = cache.Index.Where(i => i.ClassCode == "sbsp")
                .Select(i => i.ReadMetadata<scenario_structure_bsp>())
                .ToList();
        }
    }
}
