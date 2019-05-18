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

        //[DataRow("mainmenu")]
        //[DataRow("deadlock")]
        [DataRow("riverworld")]
        //[DataRow("snowbound")]
        [DataTestMethod]
        public void Halo3Beta(string map)
        {
            TestMap(BetaFolder, map);
        }

        [DataRow("020_base")]
        [DataTestMethod]
        public void Halo3Retail(string map)
        {
            TestMap(RetailSPFolder, map);
        }

        private void TestMap(string folder, string map)
        {
            var cache = new Adjutant.Blam.Halo3.CacheFile(Path.Combine(folder, $"{map}.map"));
        }
    }
}
