using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public class TexturedVertex : IVertex
    {
        private Vector3 _position;
        private Vector2 _texCoord;

        public TexturedVertex(float x, float y, float z, float u, float v)
        {
            _position = new Vector3(x, y, z);
            _texCoord = new Vector2(u, v);
        }
        public TexturedVertex(float x, float y, float z, Vector2 texCoord)
        {
            _position = new Vector3(x, y, z);
            _texCoord = texCoord;
        }
        public TexturedVertex(Vector3 position, float u, float v)
        {
            _position = position;
            _texCoord = new Vector2(u, v);
        }
        public TexturedVertex(Vector3 position, Vector2 texCoord)
        {
            _position = position;
            _texCoord = texCoord;
        }

        public Vector3 GetVertexPoint()
        {
            return Position;
        }

        public Vector3 Position => _position;
        public Vector2 TextureCoordinates => _texCoord;
    }
}
