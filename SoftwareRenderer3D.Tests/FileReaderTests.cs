using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareRenderer3D.Factories;

namespace SoftwareRenderer3D.Tests
{
    [TestClass]
    public class FileReaderTests
    {
        [TestMethod]
        public void ReadBunnyStlAscii()
        {
            var filepath = @"E:\FINKI\000Diplmoska\3DSoftwareRenderer\3DSoftwareRenderer\Models\bunny.stl";

            var mesh = FileReaderFactory.GetFileReader(filepath).ReadFile(filepath);

            Assert.IsNotNull(mesh);
            Assert.AreEqual(mesh.FacetCount, 10996);
            Assert.AreEqual(mesh.VertexCount, 5550);
        }

        [TestMethod]
        public void ReadSphereStlBinary()
        {
            var filepath = @"E:\FINKI\000Diplmoska\3DSoftwareRenderer\3DSoftwareRenderer\Models\sphere.stl";

            var mesh = FileReaderFactory.GetFileReader(filepath).ReadFile(filepath);

            Assert.IsNotNull(mesh);
            Assert.AreEqual(mesh.FacetCount, 12600);
            Assert.AreEqual(mesh.VertexCount, 6302);
        }
    }
}
