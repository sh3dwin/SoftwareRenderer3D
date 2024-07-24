using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Xml.Linq;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;

namespace SoftwareRenderer3D.Collada
{
    /// <summary>
    /// Adapted from:
    /// https://github.com/larsjarlvik/collada-parser/tree/master
    /// </summary>
    public class GeometryLoader
    {
        private static XNamespace ns = "{http://www.collada.org/2005/11/COLLADASchema}";

        private List<ColladaVertex> Vertices;
        private List<Vector3> Normals;
        private List<Vector2> Textures;
        private List<Vector3> Colors;
        private List<int> PolyList;

        private XElement xMesh;


        public GeometryLoader(XElement xMesh)
        {
            Vertices = new List<ColladaVertex>();
            PolyList = new List<int>();

            this.xMesh = xMesh;
        }

        public Mesh<IVertex> Load()
        {
            // Vertices
            var positionId = xMesh
                .Element($"{ns}vertices")
                .Element($"{ns}input")
                .Attribute("source").Value.TrimStart(new[] { '#' });

            var polylist = ReadVecArray<Vector3>(positionId);
            foreach (var poly in polylist)
                Vertices.Add(new ColladaVertex(Vertices.Count, poly));

            // Normals
            var normals = xMesh
                .Element($"{ns}polylist")
                .Elements($"{ns}input").FirstOrDefault(x => x.Attribute("semantic").Value == "NORMAL");
            if (normals != null)
            {
                var normalId = normals.Attribute("source").Value.TrimStart(new[] { '#' });

                Normals = new List<Vector3>();
                Normals = ReadVecArray<Vector3>(normalId);
            }

            // Textures
            var texCoords = xMesh
                .Element($"{ns}polylist")
                .Elements($"{ns}input").FirstOrDefault(x => x.Attribute("semantic").Value == "TEXCOORD");
            if (texCoords != null)
            {
                var texCoordId = texCoords.Attribute("source").Value.TrimStart(new[] { '#' });

                Textures = new List<Vector2>();
                Textures = ReadVecArray<Vector2>(texCoordId);
            }

            // Colors
            var colors = xMesh
                .Element($"{ns}polylist")
                .Elements($"{ns}input").FirstOrDefault(x => x.Attribute("semantic").Value == "COLOR");
            if (colors != null)
            {
                var colorId = colors.Attribute("source").Value.TrimStart(new[] { '#' });

                Colors = new List<Vector3>();
                Colors = ReadVecArray<Vector3>(colorId);
            }

            AssembleVertices();
            RemoveUnusedVertices();

            return convertDataToArrays();
        }

        private List<T> ReadVecArray<T>(string id)
        {
            var data = xMesh
                .Elements($"{ns}source").FirstOrDefault(x => x.Attribute("id").Value == id)
                .Element($"{ns}float_array");

            var count = int.Parse(data.Attribute("count").Value);
            var array = ArrayParsers.ParseFloats(data.Value);
            var result = new List<T>();

            if (typeof(T) == typeof(Vector3))
                for (var i = 0; i < count / 3; i++)
                    result.Add((T)(object)new Vector3(
                        array[i * 3],
                        array[i * 3 + 2],
                        array[i * 3 + 1]
                    ));
            else if (typeof(T) == typeof(Vector2))
                for (var i = 0; i < count / 2; i++)
                    result.Add((T)(object)new Vector2(
                        array[i * 2],
                        array[i * 2 + 1]
                    ));

            return result;
        }

        private void AssembleVertices()
        {
            var poly = xMesh.Element($"{ns}polylist");
            var typeCount = poly.Elements($"{ns}input").Count();
            var id = ArrayParsers.ParseInts(poly.Element($"{ns}p").Value);

            for (int i = 0; i < id.Count / typeCount; i++)
            {
                var textureIndex = -1;
                var colorIndex = -1;
                var index = 0;

                var posIndex = id[i * typeCount + index]; index++;
                var normalIndex = id[i * typeCount + index]; index++;

                if (Textures != null)
                {
                    textureIndex = id[i * typeCount + index]; index++;
                }

                if (Colors != null)
                {
                    colorIndex = id[i * typeCount + index]; index++;
                }

                ProcessVertex(posIndex, normalIndex, textureIndex, colorIndex);
            }
        }

        private void ProcessVertex(int posIndex, int normalIndex, int textureIndex, int colorIndex)
        {
            var currentVertex = Vertices[posIndex];

            if (!currentVertex.IsSet)
            {
                currentVertex.NormalIndex = normalIndex;
                currentVertex.TextureIndex = textureIndex;
                currentVertex.ColorIndex = colorIndex;
                PolyList.Add(posIndex);
            }
            else
            {
                HandleAlreadyProcessedVertex(currentVertex, normalIndex, textureIndex, colorIndex);
            }
        }

        private void HandleAlreadyProcessedVertex(ColladaVertex previousVertex, int newNormalIndex, int newTextureIndex, int newColorIndex)
        {
            if (previousVertex.HasSameInformation(newNormalIndex, newTextureIndex, newColorIndex))
            {
                PolyList.Add(previousVertex.Index);
                return;
            }

            if (previousVertex.DuplicateVertex != null)
            {
                HandleAlreadyProcessedVertex(previousVertex.DuplicateVertex, newNormalIndex, newTextureIndex, newColorIndex);
                return;
            }

            var duplicateVertex = new ColladaVertex(Vertices.Count, previousVertex.Position);

            duplicateVertex.NormalIndex = newNormalIndex;
            duplicateVertex.TextureIndex = newTextureIndex;
            duplicateVertex.ColorIndex = newColorIndex;
            previousVertex.DuplicateVertex = duplicateVertex;

            Vertices.Add(duplicateVertex);
            PolyList.Add(duplicateVertex.Index);
        }

        private void RemoveUnusedVertices()
        {
            foreach (var vertex in Vertices)
            {
                if (!vertex.IsSet)
                {
                    vertex.NormalIndex = 0;
                    vertex.TextureIndex = 0;
                    vertex.ColorIndex = 0;
                }
            }
        }

        private Mesh<IVertex> convertDataToArrays()
        {
            var verticesArray = new Vector3[Vertices.Count];
            var normalsArray = new Vector3[Vertices.Count];

            Vector2[] texturesArray = null;
            Vector3[] colorsArray = null;

            if (Textures != null)
            {
                texturesArray = new Vector2[Vertices.Count];
            }

            if (Colors != null)
            {
                colorsArray = new Vector3[Vertices.Count];
            }

            for (int i = 0; i < Vertices.Count; i++)
            {
                ColladaVertex currentVertex = Vertices[i];

                verticesArray[i] = currentVertex.Position;
                normalsArray[i] = Normals[currentVertex.NormalIndex];

                if (texturesArray != null) texturesArray[i] = Textures[currentVertex.TextureIndex];
                if (colorsArray != null) colorsArray[i] = Colors[currentVertex.ColorIndex];
            }

            var maxLength = verticesArray.Max(x => x.Length());

            var facets = new Dictionary<int, Facet>(Vertices.Count);
            var vertices = new Dictionary<int, IVertex>(Vertices.Count);

            for (var i = 0; i < Vertices.Count; i++)
            {
                var vertex = new TexturedVertex(verticesArray[i] / maxLength, normalsArray[i], texturesArray[i], colorsArray[i]);
                vertices[vertices.Count] = vertex;
            }

            for (var i = 0; i < Vertices.Count; i += 3)
            {
                Facet facet = new Facet(PolyList[i], PolyList[i + 1], PolyList[i + 2], (normalsArray[PolyList[i]] + normalsArray[PolyList[i + 1]] + normalsArray[PolyList[i + 2]]) / 3);
                facets.Add(i / 3, facet);
            }

            return new Mesh<IVertex>(vertices, facets);
        }
    }
}