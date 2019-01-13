using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Linq;
using Adjutant.IO;
using Adjutant.Blam.Halo2;

namespace Adjutant.Tests.Blam.Halo2
{
    [TestClass]
    public class CacheFile
    {
        private const string MapsFolder = @"Y:\Halo\Halo2Xbox\maps";

        [DataRow("01b_spacestation")]
        [DataTestMethod]
        public void Halo2Campaign(string map)
        {
            var cache = new Adjutant.Blam.Halo2.CacheFile(Path.Combine(MapsFolder, $"{map}.map"));

            var bitmaps = cache.Index.Where(i => i.ClassCode == "bitm")
                .Select(i => i.ReadMetadata<bitmap>())
                .ToList();

            var models = cache.Index.Where(i => i.ClassCode == "mode")
                .Select(i => i.ReadMetadata<render_model>())
                .ToList();

            //var bsps = cache.Index.Where(i => i.ClassCode == "sbsp")
            //    .Select(i => i.ReadMetadata<scenario_structure_bsp>())
            //    .ToList();
        }

        //[DataRow("")]
        //[DataTestMethod]
        //public void Halo2Multiplayer(string map)
        //{
        //    var cache = new Adjutant.Blam.Halo1.CacheFile(Path.Combine(MapsFolder, $"{map}.map"));

        //    var models = cache.Index.Where(i => i.ClassCode == "mode")
        //        .Select(i => i.ReadMetadata<render_model>())
        //        .ToList();

        //    var bsps = cache.Index.Where(i => i.ClassCode == "sbsp")
        //        .Select(i => i.ReadMetadata<scenario_structure_bsp>())
        //        .ToList();
        //}
    }
}
