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
        private Vector3 _center;
        private Matrix4x4 _modelMatrix;



        public Mesh(Dictionary<int, V> vertices, Dictionary<int, Facet> facets) {
            Vertices = vertices;
            Facets = facets;

            _center = GetCenterOfMass();
            _modelMatrix = Matrix4x4.Transpose(Matrix4x4.CreateTranslation(_center));
        }

        public Mesh(Mesh<V> otherMesh)
        {
            Vertices = otherMesh.Vertices;
            Facets = otherMesh.Facets;
        }
        public int VertexCount => Vertices.Count;
        public int FacetCount => Facets.Count;
        public Matrix4x4 ModelMatrix => _modelMatrix;

        public IEnumerable<Facet> GetFacets()
        {
            return Facets.Values;
        }

        public Vector3 GetVertexPoint(int index)
        {
            return Vertices[index].GetVertexPoint();
        }

        public Vector3 GetFacetMidpoint(int index)
        {
            return (GetVertexPoint(Facets[index].V0) + GetVertexPoint(Facets[index].V1) + GetVertexPoint(Facets[index].V2)) / 3.0f;
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

        public Vector3 GetCenterOfMass()
        {
            var sum = Vector3.Zero;

            foreach(var vertex in Vertices.Values)
            {
                sum += vertex.GetVertexPoint();
            }

            return sum / Vertices.Count;
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
