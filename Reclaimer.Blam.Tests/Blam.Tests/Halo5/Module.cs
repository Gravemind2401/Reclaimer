namespace Reclaimer.Blam.Tests.Halo5
{
    [TestClass]
    public class Module
    {
        [DataRow("globals-rtx-1")]
        [DataTestMethod]
        public void Halo5Forge(string fileName)
        {
            TestModule(Directories.Halo5Forge, fileName);
        }

        private void TestModule(string folder, string fileName)
        {
            var module = new Blam.Halo5.Module(Path.Combine(folder, $"{fileName}.module"));
        }
    }
}
