using g3;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public class StandardVertex : IVertex
    {
        private Vector3f _position;

        public StandardVertex(Vector3f position)
        {
            _position = position;
        }
        public Vector3f GetVertexPoint()
        {
            return _position;
        }
    }
}
