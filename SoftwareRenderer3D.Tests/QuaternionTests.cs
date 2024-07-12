using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftwareRenderer3D.Maths;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Tests
{
    [TestClass]
    public class QuaternionTests
    {
        [TestMethod]
        public void RotationOnXYPlaneTest()
        {
            var firstVector = Vector3.UnitX;
            var secondVector = Vector3.UnitY;

            var position = new Vector3(100, 0, 0);

            var rotation = Maths.Quaternion.FromBetweenVectors(firstVector, secondVector);

            var goalPosition = new Vector3(0, 100, 0);

            var rotatedVector = (rotation * new Maths.Quaternion(0, position) * rotation.Conjugate()).Imaginary;

            Assert.AreEqual(rotatedVector, goalPosition);
        }

        [TestMethod]
        public void RotationOnXZPlaneTest()
        {
            var firstVector = Vector3.UnitX;
            var secondVector = Vector3.UnitZ;

            var position = new Vector3(100, 0, 0);

            var rotation = Maths.Quaternion.FromBetweenVectors(firstVector, secondVector);

            var goalPosition = new Vector3(0, 0, 100);

            var rotatedVector = (rotation * new Maths.Quaternion(0, position) * rotation.Conjugate()).Imaginary;

            Assert.AreEqual(rotatedVector, goalPosition);
        }

        [TestMethod]
        public void RotationOnYZPlaneTest()
        {
            var firstVector = Vector3.UnitY;
            var secondVector = Vector3.UnitZ;

            var position = new Vector3(0, 100, 0);

            var rotation = Maths.Quaternion.FromBetweenVectors(firstVector, secondVector);

            var goalPosition = new Vector3(0, 0, 100);

            var rotatedVector = (rotation * new Maths.Quaternion(0, position) * rotation.Conjugate()).Imaginary;

            Assert.AreEqual(rotatedVector, goalPosition);
        }

        [TestMethod]
        public void DoubleRotation()
        {
            var firstVector = Vector3.UnitX;
            var secondVector = Vector3.UnitY;

            var position = new Vector3(100, 0, 0);

            var rotation = Maths.Quaternion.FromBetweenVectors(firstVector, secondVector);

            var goalPosition = new Vector3(-100, 0, 0);

            var rotatedVector = (rotation * new Maths.Quaternion(0, position) * rotation.Conjugate()).Imaginary;
            rotatedVector = (rotation * new Maths.Quaternion(0, rotatedVector) * rotation.Conjugate()).Imaginary;

            Assert.AreEqual(rotatedVector, goalPosition);
        }

        [TestMethod]
        public void RotationByMultiplyingQuaternions()
        {
            var firstVector = Vector3.UnitX;
            var secondVector = Vector3.UnitY;

            var position = new Vector3(100, 0, 0);

            var rotation = Maths.Quaternion.FromBetweenVectors(firstVector, secondVector);

            var goalPosition = new Vector3(-100, 0, 0);

            rotation *= rotation;

            var rotatedVector = (rotation * new Maths.Quaternion(0, position) * rotation.Conjugate()).Imaginary;

            Assert.AreEqual(rotatedVector, goalPosition);
        }
    }
}
