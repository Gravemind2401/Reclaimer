using Adjutant.Blam.Halo5;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Adjutant.Tests.Blam.Halo5
{
    [TestClass]
    public class Module
    {
        private const string ServerFolder = @"Y:\Halo\Halo5Server\deploy\any\levels";
        private const string ForgeFolder = @"Y:\Halo\Halo5Forge\deploy\any\levels";

        [DataRow("globals-rtx-1")]
        [DataTestMethod]
        public void Halo5Forge(string fileName)
        {
            TestModule(ForgeFolder, fileName);
        }

        private void TestModule(string folder, string fileName)
        {
            var module = new Adjutant.Blam.Halo5.Module(Path.Combine(folder, $"{fileName}.module"));
        }
    }
}
