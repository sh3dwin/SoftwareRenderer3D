using g3;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections.Generic;

namespace SoftwareRenderer3D.DataStructures.MeshDataStructures
{
    /// <summary>
    /// Describes the base class for the Mesh data structure
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class Mesh<V>
        where V : IVertex
    {
        protected Dictionary<int, V> _vertices { get; set; }
        protected Dictionary<int, Facet> _facets { get; set; }

        public Mesh(Dictionary<int, V> vertices, Dictionary<int, Facet> facets) {

        }
        public int VertexCount { get; }
        public int FacetCount { get; }

        public IEnumerable<Facet> GetFacets()
        {
            return _facets.Values;
        }

        public Vector3f GetVertexPoint(int index)
        {
            return _vertices[index].GetVertexPoint();
        }

        public Vector3f GetFacetNormal(int index)
        {
            return _facets[index].Normal;
        }
        public V GetVertex(int index)
        {
            return _vertices[index];
        }
    }
}
