using System.Numerics;
using System.Drawing;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public class ColoredVertex: StandardVertex
    {
        private Color _color;

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
            }
        }

        public ColoredVertex(Vector3 position, Color color) : base(position)
        {
            _color = color;
        }
    }
}
