using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace SoftwareRenderer3D.DataStructures.MeshDataStructures
{
    /// <summary>
    /// Describes the base class for the Mesh data structure
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public abstract class Mesh<V>
        where V : IVertex
    {
        protected Dictionary<int, V> _vertices { get; set; }
        protected List<int> _facets { get; set; }

        public abstract int VertexCount { get; }
        public abstract int FacetCount { get; }

        public abstract List<int> GetFacets();

        public abstract Vector3D GetVertexPoint(int index);

        public abstract Vector3D GetFacetNormal(int index);
        public abstract V GetVertex(int index);
    }
}
