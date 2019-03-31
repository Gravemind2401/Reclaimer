using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Microsoft.Win32;

namespace Reclaimer.Tests
{
    [TestClass]
    public class UnitTest1
    {
        private const string MapsFolder = @"Y:\Halo\Halo1PC\MAPS";

        [TestMethod]
        public void TestMethod1()
        {
            var fsd = new OpenFileDialog
            {
                InitialDirectory = MapsFolder,
                Filter = "Map Files (*.map)|*.map"
            };

            if (fsd.ShowDialog() == true)
                Reclaimer.Storage.ImportCacheFile(fsd.FileName).GetAwaiter().GetResult();
        }
    }
}
