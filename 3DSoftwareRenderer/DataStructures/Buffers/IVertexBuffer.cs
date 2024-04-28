using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.Buffers
{
    public interface IVertexBuffer
    {
        void TransformVertices(Matrix4x4 modelViewMatrix, Matrix4x4 projectionMatrix);
    }
}
