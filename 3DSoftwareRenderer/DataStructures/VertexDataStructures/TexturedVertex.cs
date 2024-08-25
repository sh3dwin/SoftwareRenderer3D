using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Drawing;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public class TexturedVertex : IVertex
    {
        private Vector3 _normal;
        private Vector3 _worldPosition;
        private Vector3 _ndcPosition;
        private Vector3 _screenPosition;
        private Color _color;

        private Vector2 _texCoord;

        public TexturedVertex(Vector3 normal, float x, float y, float z, float u, float v, float r, float g, float b, float a)
        {
            _normal = normal;

            var R = (byte)(MathUtils.Clamp(r, 0f, 1f) * 255);
            var G = (byte)(MathUtils.Clamp(g, 0f, 1f) * 255);
            var B = (byte)(MathUtils.Clamp(b, 0f, 1f) * 255);
            var A = (byte)(MathUtils.Clamp(a, 0f, 1f) * 255);
            _color = Color.FromArgb(A, R, G, B);
            _worldPosition = new Vector3(x, y, z);
            _texCoord = new Vector2(u, v);
        }

        public TexturedVertex(Vector3 position, Vector3 normal, float u, float v)
        {
            _normal = normal;
            _color = Color.Transparent;
            _worldPosition = position;
            _texCoord = new Vector2(u, v);
        }
        public TexturedVertex(Vector3 position, Vector3 normal, Vector2 texCoord)
        {
            _normal = normal;
            _color = Color.Transparent;
            _worldPosition = position;
            _texCoord = texCoord;
        }

        public TexturedVertex(Vector3 position, Vector3 normal, Vector2 texCoord, Color color)
        {
            _normal = normal;
            _color = color;
            _worldPosition = position;
            _texCoord = texCoord;
        }

        public TexturedVertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color)
        {
            var R = (byte)(MathUtils.Clamp(color.X, 0f, 1f) * 255);
            var G = (byte)(MathUtils.Clamp(color.Y, 0f, 1f) * 255);
            var B = (byte)(MathUtils.Clamp(color.Z, 0f, 1f) * 255);

            _normal = normal;
            _color = Color.FromArgb(R, G, B);
            _worldPosition = position;
            _texCoord = texCoord;
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

        public void SetScreenCoordinates(int width, int height)
        {
            _screenPosition = new Vector3((_ndcPosition.X + 1) * width / 2.0f, (-_ndcPosition.Y + 1) * height / 2.0f, _ndcPosition.Z);
        }
        public void SetNormal(Vector3 normal)
        {
            _normal = normal;
        }
        public Vector2 TextureCoordinates => _texCoord;
    }
}
