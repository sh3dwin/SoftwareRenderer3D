using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.DataStructures.MeshDataStructures
{
    /// <summary>
    /// Describes the base class for the Mesh data structure
    /// </summary>
    /// <typeparam name="V"></typeparam>
    public class Mesh<V> : IEquatable<Mesh<V>>
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
            Matrix4x4.Invert(Matrix4x4.Transpose(Matrix4x4.CreateTranslation(_center)), out _modelMatrix);
        }

        public Mesh(Mesh<V> otherMesh)
        {
            _vertices = otherMesh._vertices;
            _facets = otherMesh._facets;
        }

        public IEnumerable<V> Vertices => _vertices.Values;
        public IEnumerable<Facet> Facets => _facets.Values;
        public IEnumerable<int> VertexIds => _vertices.Keys;
        public IEnumerable<int> FacetIds => _facets.Keys;

        public int VertexCount => _vertices.Count;
        public int FacetCount => _facets.Count;
        public Matrix4x4 ModelMatrix => _modelMatrix;

        public IEnumerable<Facet> GetFacets()
        {
            return _facets.Values;
        }

        public Vector3 GetVertexPoint(int index)
        {
            return _vertices[index].WorldPoint;
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

            foreach (var vertex in _vertices.Values)
            {
                sum += vertex.WorldPoint;
            }

            return sum / _vertices.Count;
        }

        // Needs fixing
        public Vector3 GeometricCenter()
        {
            (var first, var second) = _vertices.Values.Select(v => v.WorldPoint).BoundingBox();

            return 0.5f * (first + second);
        }

        public void EnsureMeshQuality()
        {
            var vertexOccurrences = new Dictionary<Vector3, int>();
            var positionVertexId = new Dictionary<Vector3, int>();
            var normalsMapping = new Dictionary<Vector3, Vector3>();

            foreach (var facet in _facets.Values)
            {
                var v0 = _vertices[facet.V0];
                var v1 = _vertices[facet.V1];
                var v2 = _vertices[facet.V2];

                var normal = Vector3.Cross(Vector3.Normalize(v2.WorldPoint - v0.WorldPoint), Vector3.Normalize(v1.WorldPoint - v0.WorldPoint));

                facet.UpdateNormal(normal);

                if (!vertexOccurrences.ContainsKey(v0.WorldPoint))
                {
                    vertexOccurrences[v0.WorldPoint] = 0;
                    normalsMapping[v0.WorldPoint] = Vector3.Zero;
                    positionVertexId[v0.WorldPoint] = facet.V0;
                }
                vertexOccurrences[v0.WorldPoint]++;
                normalsMapping[v0.WorldPoint] += facet.Normal;

                if (!vertexOccurrences.ContainsKey(v1.WorldPoint))
                {
                    vertexOccurrences[v1.WorldPoint] = 0;
                    normalsMapping[v1.WorldPoint] = Vector3.Zero;
                    positionVertexId[v1.WorldPoint] = facet.V1;
                }
                vertexOccurrences[v1.WorldPoint]++;
                normalsMapping[v1.WorldPoint] += facet.Normal;

                if (!vertexOccurrences.ContainsKey(v2.WorldPoint))
                {
                    vertexOccurrences[v2.WorldPoint] = 0;
                    normalsMapping[v2.WorldPoint] = Vector3.Zero;
                    positionVertexId[v2.WorldPoint] = facet.V2;
                }
                vertexOccurrences[v2.WorldPoint]++;
                normalsMapping[v2.WorldPoint] += facet.Normal;
            }

            foreach (var veId in VertexIds)
            {
                var position = _vertices[veId].WorldPoint;
                normalsMapping[position] /= vertexOccurrences[position];
                _vertices[veId].SetNormal(normalsMapping[position]);
            }


            var positionToIdMapping = new Dictionary<Vector3, int>();

            var index = 0;
            foreach(var position in vertexOccurrences.Keys)
            {
                positionToIdMapping[position] = index++;
            }

            var newFacets = new Dictionary<int, Facet>();

            index = 0;
            foreach (var facetId in _facets.Keys)
            {
                var veId0 = positionToIdMapping[_vertices[_facets[facetId].V0].WorldPoint];
                var veId1 = positionToIdMapping[_vertices[_facets[facetId].V1].WorldPoint];
                var veId2 = positionToIdMapping[_vertices[_facets[facetId].V2].WorldPoint];

                newFacets[index++] = new Facet(veId0, veId1, veId2, _facets[facetId].Normal);
            }

            var newVertices = new Dictionary<int, V>(vertexOccurrences.Count);
            foreach(var keyValue in positionToIdMapping)
            {
                newVertices[keyValue.Value] = _vertices[positionVertexId[keyValue.Key]];
                newVertices[keyValue.Value].Color = Color.White;
            }

            _vertices = newVertices;
            _facets = newFacets;
        }

        public void TransformVertices(int width, int height, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix)
        {
            Parallel.ForEach(VertexIds, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, vertexId =>
            {
                var vertex = _vertices[vertexId];
                var modelV0 = _vertices[vertexId].WorldPoint.TransformHomogeneus(ModelMatrix);
                modelV0 /= modelV0.W;

                var viewV0 = modelV0.Transform(viewMatrix);
                viewV0 /= viewV0.W;

                var clipV0 = viewV0.Transform(projectionMatrix);
                var ndcV0 = clipV0 / clipV0.W;

                _vertices[vertexId].NDCPosition = ndcV0.ToVector3();
                _vertices[vertexId].SetScreenCoordinates(width, height);
            });
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
