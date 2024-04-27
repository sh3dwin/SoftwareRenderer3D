using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SoftwareRenderer3D.DataStructures.VertexDataStructures
{
    public interface IVertex
    {
        Vector3D GetVertexPoint();
    }
}
