using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace _3DSoftwareRenderer.DataStructures.VertexDataStructures
{
    public class StandardVertex : IVertex
    {
        private Vector3D _position;

        public StandardVertex(Vector3D position)
        {
            _position = position;
        }
        public Vector3D GetVertexPoint()
        {
            return _position;
        }

    }
}
