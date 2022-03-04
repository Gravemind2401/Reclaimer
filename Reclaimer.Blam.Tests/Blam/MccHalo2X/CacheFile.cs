using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reclaimer.Blam.Halo4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Tests.Blam.MccHalo2X
{
    [TestClass]
    public class CacheFile
    {
        private const string MapsFolder = @"Y:\Program Files (x86)\Steam\steamapps\common\Halo The Master Chief Collection\groundhog\maps";

        [DataRow("ca_ascension")]
        [DataRow("ca_coagulation")]
        [DataRow("ca_forge_skybox01")]
        [DataRow("ca_forge_skybox02")]
        [DataRow("ca_forge_skybox03")]
        [DataRow("ca_lockout")]
        [DataRow("ca_relic")]
        [DataRow("ca_sanctuary")]
        [DataRow("ca_warlock")]
        [DataRow("ca_zanzibar")]
        [DataTestMethod]
        public void Halo2X(string map)
        {
            TestMap(MapsFolder, map);
        }

        private void TestMap(string folder, string map)
        {
            var cache = new Reclaimer.Blam.MccHalo2X.CacheFile(Path.Combine(folder, $"{map}.map"));

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

            //var t2 = Task.Run(() =>
            //{
            //    var models = cache.TagIndex.Where(i => i.ClassCode == "mode")
            //    .Select(i => i.ReadMetadata<render_model>())
            //    .ToList();

            //    return true;
            //});

            //var t3 = Task.Run(() =>
            //{
            //    var bsps = cache.TagIndex.Where(i => i.ClassCode == "sbsp")
            //    .Select(i => i.ReadMetadata<scenario_structure_bsp>())
            //    .ToList();

            //    return true;
            //});

            Assert.IsTrue(t0.GetAwaiter().GetResult());
            Assert.IsTrue(t1.GetAwaiter().GetResult());
            //Assert.IsTrue(t2.GetAwaiter().GetResult());
            //Assert.IsTrue(t3.GetAwaiter().GetResult());
        }
    }
}
