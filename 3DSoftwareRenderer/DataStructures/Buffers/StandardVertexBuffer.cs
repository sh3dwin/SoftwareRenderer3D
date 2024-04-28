using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.Buffers
{
    public class StandardVertexBuffer : IVertexBuffer
    {
        private Dictionary<int, StandardVertex> _vertices;
        private Dictionary<int, StandardVertex> _viewVertices;
        private Dictionary<int, StandardVertex> _projectionVertices;

        public StandardVertexBuffer(Dictionary<int, StandardVertex> vertices) {
            _vertices = new Dictionary<int, StandardVertex>(vertices);
        }

        public Dictionary<int, StandardVertex> Vertices => _vertices;
        public Dictionary<int, StandardVertex> ViewVertices => _viewVertices;
        public Dictionary<int, StandardVertex> NDCVertices => _projectionVertices;

        public void TransformVertices(Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            _viewVertices = TransformVertices(viewMatrix);
            _projectionVertices = TransformVertices(projectionMatrix);
        }

        private Dictionary<int, StandardVertex> TransformVertices(Matrix4x4 transformation)
        {
            var vertices = _vertices.Values;
            var transformedVertices = vertices.Select(vertex => new StandardVertex(Vector3.Transform(vertex.GetVertexPoint(), transformation)));
            var result = new Dictionary<int, StandardVertex>(vertices.Count);
            foreach (var vertex in transformedVertices)
            {
                result.Add(result.Count, vertex);
            }

            return result;
        }
    }
}
