using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Drawing;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public class TexturedVertex : IVertex
    {
        private Vector3 _position;
        private Vector2 _texCoord;
        private Color _color;

        public TexturedVertex(float x, float y, float z, float u, float v, float r, float g, float b, float a)
        {
            var R = (byte)(MathUtils.Clamp(r, 0f, 1f) * 255);
            var G = (byte)(MathUtils.Clamp(g, 0f, 1f) * 255);
            var B = (byte)(MathUtils.Clamp(b, 0f, 1f) * 255);
            var A = (byte)(MathUtils.Clamp(a, 0f, 1f) * 255);
            _color = Color.FromArgb(A, R, G, B);
            _position = new Vector3(x, y, z);
            _texCoord = new Vector2(u, v);
        }
        public TexturedVertex(float x, float y, float z, Vector2 texCoord, Color color)
        {
            _color = color;
            _position = new Vector3(x, y, z);
            _texCoord = texCoord;
        }
        public TexturedVertex(Vector3 position, float u, float v)
        {
            _color = Color.Transparent;
            _position = position;
            _texCoord = new Vector2(u, v);
        }
        public TexturedVertex(Vector3 position, Vector2 texCoord)
        {
            _color = Color.Transparent;
            _position = position;
            _texCoord = texCoord;
        }

        public TexturedVertex(Vector3 position, Vector2 texCoord, Color color)
        {
            _color = color;
            _position = position;
            _texCoord = texCoord;
        }

        public TexturedVertex(Vector3 position, Vector2 texCoord, Vector3 color)
        {
            var R = (byte)(MathUtils.Clamp(color.X, 0f, 1f) * 255);
            var G = (byte)(MathUtils.Clamp(color.Y, 0f, 1f) * 255);
            var B = (byte)(MathUtils.Clamp(color.Z, 0f, 1f) * 255);

            _color = Color.FromArgb(R, G, B);
            _position = position;
            _texCoord = texCoord;
        }
        public Vector3 GetVertexPoint()
        {
            return Position;
        }

        public int ARGB => _color.ToArgb();
        public Color Color => _color;

        public Vector3 Position => _position;
        public Vector2 TextureCoordinates => _texCoord;
    }
}
