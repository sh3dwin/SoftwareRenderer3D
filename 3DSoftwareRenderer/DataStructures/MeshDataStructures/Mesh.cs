using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections.Generic;
using System.Numerics;

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
            _vertices = vertices;
            _facets = facets;
        }
        public int VertexCount => _vertices.Count;
        public int FacetCount => _facets.Count;

        public IEnumerable<Facet> GetFacets()
        {
            return _facets.Values;
        }

        public Vector3 GetVertexPoint(int index)
        {
            return _vertices[index].GetVertexPoint();
        }

        public Facet GetFacet(int index)
        {
            return _facets[index];
        }

        public Vector3 GetFacetNormal(int index)
        {
            return _facets[index].Normal;
        }
        public V GetVertex(int index)
        {
            return _vertices[index];
        }

        public void RecalculateNormals()
        {
            foreach(var facet in _facets.Values)
            {
                var v0 = _vertices[facet.V0].GetVertexPoint();
                var v1 = _vertices[facet.V1].GetVertexPoint();
                var v2 = _vertices[facet.V2].GetVertexPoint();

                var normal = Vector3.Cross(Vector3.Normalize(v2 - v0), Vector3.Normalize(v1 - v0));

                facet.UpdateNormal(normal);
            }
        }
    }
}
