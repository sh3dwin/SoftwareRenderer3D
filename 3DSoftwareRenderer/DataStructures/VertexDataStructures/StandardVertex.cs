using System.Drawing;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public class StandardVertex : IVertex
    {
        private Vector3 _normal;
        private Vector3 _worldPosition;
        private Vector3 _ndcPosition;
        private Vector3 _screenPosition;
        private Color _color;

        public StandardVertex(Vector3 position)
        {
            _worldPosition = position;
        }

        public StandardVertex(Vector3 position, Vector3 normal)
        {
            _normal = normal;
            _worldPosition = position;
        }

        public StandardVertex(float x, float y, float z, Vector3 normal)
        {
            _normal = normal;
            _worldPosition = new Vector3(x, y, z);
            _ndcPosition = Vector3.Zero;
            _screenPosition = Vector3.Zero;
        }

        public Vector3 Normal => _normal;
        public Vector3 WorldPoint
        {
            get => _worldPosition;
            set
            {
                _worldPosition = value;
            }
        }

        public Color Color
        {
            get => _color;
            set => _color = value;
        }

        public Vector3 NDCPosition
        {
            get => _ndcPosition;
            set => _ndcPosition = value;
        }
        public Vector3 ScreenPosition => _screenPosition;

        public void SetNormal(Vector3 normal)
        {
            _normal = normal;
        }

        public void SetScreenCoordinates(int width, int height)
        {
            _screenPosition = new Vector3((_ndcPosition.X + 1) * width / 2.0f, (-_ndcPosition.Y + 1) * height / 2.0f, _ndcPosition.Z);
        }
    }
}
