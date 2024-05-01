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
        private Dictionary<int, V> Vertices { get; set; }
        private Dictionary<int, Facet> Facets { get; set; }

        public Mesh(Dictionary<int, V> vertices, Dictionary<int, Facet> facets) {
            Vertices = vertices;
            Facets = facets;
        }
        public int VertexCount => Vertices.Count;
        public int FacetCount => Facets.Count;

        public IEnumerable<Facet> GetFacets()
        {
            return Facets.Values;
        }

        public Vector3 GetVertexPoint(int index)
        {
            return Vertices[index].GetVertexPoint();
        }

        public Facet GetFacet(int index)
        {
            return Facets[index];
        }

        public Vector3 GetFacetNormal(int index)
        {
            return Facets[index].Normal;
        }
        public V GetVertex(int index)
        {
            return Vertices[index];
        }

        public void RecalculateNormals()
        {
            foreach(var facet in Facets.Values)
            {
                var v0 = Vertices[facet.V0].GetVertexPoint();
                var v1 = Vertices[facet.V1].GetVertexPoint();
                var v2 = Vertices[facet.V2].GetVertexPoint();

                var normal = Vector3.Cross(Vector3.Normalize(v2 - v0), Vector3.Normalize(v1 - v0));

                facet.UpdateNormal(normal);
            }
        }
    }
}
