using Reclaimer.Geometry.Vectors;

namespace Reclaimer.Blam.Tests.Halo5
{
    [TestClass]
    public class DqBlendWeights
    {
        [TestMethod]
        public void ReconstructsPackedShaderWeights()
        {
            const uint bits = 0x7EEAD501;
            var packed = new UDecN4(bits);
            var actual = Blam.Halo5.Halo5Common.ReconstructDqBlendWeights(packed);

            var scale = (((bits >> 30) & 3) / 3f) * 0.5f;
            var expectedX = ((bits >> 0) & 1023) / 1023f * scale;
            var expectedY = ((bits >> 10) & 1023) / 1023f * scale;
            var expectedZ = ((bits >> 20) & 1023) / 1023f * scale;
            var expectedW = 1f - expectedX - expectedY - expectedZ;

            Assert.AreEqual(expectedX, actual.X, 1e-7f);
            Assert.AreEqual(expectedY, actual.Y, 1e-7f);
            Assert.AreEqual(expectedZ, actual.Z, 1e-7f);
            Assert.AreEqual(expectedW, actual.W, 1e-7f);
            Assert.AreEqual(1f, actual.X + actual.Y + actual.Z + actual.W, 1e-7f);
        }

        [TestMethod]
        public void ZeroAlphaMakesFourthInfluenceRigid()
        {
            var actual = Blam.Halo5.Halo5Common.ReconstructDqBlendWeights(new UDecN4(0x3FFFFFFF));

            Assert.AreEqual(0f, actual.X);
            Assert.AreEqual(0f, actual.Y);
            Assert.AreEqual(0f, actual.Z);
            Assert.AreEqual(1f, actual.W);
        }
    }
}
