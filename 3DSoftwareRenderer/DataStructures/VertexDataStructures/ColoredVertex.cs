using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace _3DSoftwareRenderer.DataStructures.VertexDataStructures
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

        public ColoredVertex(Vector3D position, Color color) : base(position)
        {
            _color = color;
        }
    }
}
