using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.DataStructures.Buffers
{
    public interface IVertexBuffer
    {
        void TransformVertices(Matrix4x4 modelViewMatrix, Matrix4x4 projectionMatrix);
    }
}
