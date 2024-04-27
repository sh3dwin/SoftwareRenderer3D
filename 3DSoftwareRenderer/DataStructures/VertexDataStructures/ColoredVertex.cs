using g3;
using System.Windows.Media;

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

        public ColoredVertex(Vector3f position, Color color) : base(position)
        {
            _color = color;
        }
    }
}
