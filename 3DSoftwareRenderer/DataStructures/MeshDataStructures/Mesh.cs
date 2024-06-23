using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.MeshDataStructures
{
    /// <summary>
    /// Describes the base class for the Mesh data structure
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class Mesh<V>: IEquatable<Mesh<V>>
        where V : IVertex
    {
        private Dictionary<int, V> _vertices { get; set; }
        private Dictionary<int, Facet> _facets { get; set; }
        private Vector3 _center;
        private Matrix4x4 _modelMatrix;



        public Mesh(Dictionary<int, V> vertices, Dictionary<int, Facet> facets) {
            _vertices = vertices;
            _facets = facets;

            _center = GetCenterOfMass();
            _modelMatrix = Matrix4x4.Transpose(Matrix4x4.CreateTranslation(_center));
        }

        public Mesh(Mesh<V> otherMesh)
        {
            _vertices = otherMesh._vertices;
            _facets = otherMesh._facets;
        }

        public IEnumerable<V> Vertices => _vertices.Values;
        public IEnumerable<Facet> Facets => _facets.Values;

        public int VertexCount => _vertices.Count;
        public int FacetCount => _facets.Count;
        public Matrix4x4 ModelMatrix => _modelMatrix;

        public IEnumerable<Facet> GetFacets()
        {
            return _facets.Values;
        }

        public Vector3 GetVertexPoint(int index)
        {
            return _vertices[index].GetVertexPoint();
        }

        public Vector3 GetFacetMidpoint(int index)
        {
            return (GetVertexPoint(_facets[index].V0) + GetVertexPoint(_facets[index].V1) + GetVertexPoint(_facets[index].V2)) / 3.0f;
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

        public Vector3 GetCenterOfMass()
        {
            var sum = Vector3.Zero;

            foreach(var vertex in _vertices.Values)
            {
                sum += vertex.GetVertexPoint();
            }

            return sum / _vertices.Count;
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

        bool IEquatable<Mesh<V>>.Equals(Mesh<V> other)
        {
            if(other == null) return false;
            if(other.FacetCount != FacetCount) return false;
            if(other.VertexCount != VertexCount) return false;

            return true;
        }
    }
}
