using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using System;
using System.IO;
using System.Collections.Generic;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using System.Numerics;

namespace SoftwareRenderer3D.FileReaders
{
    public class STLReader : IMeshFileReader
    {
        public Mesh<IVertex> ReadFile(string path)
        {
            return ReadStlFile(path);
        }

        private Mesh<IVertex> ReadStlFile(string path)
        {
            try
            {
                var file = File.OpenRead(path);
                
                using ( var reader = new StreamReader(file))
                {
                    var firstLine = reader.ReadLine();

                    if(firstLine.StartsWith("solid"))
                        return ReadASCII(path);
                    else
                        return ReadBinary(path);
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        private Mesh<IVertex> ReadBinary(string path) { throw new NotImplementedException(); }
        private Mesh<IVertex> ReadASCII(string path) {

            Dictionary<Vector3, int> veIds = new Dictionary<Vector3, int>();
            Dictionary<int, Facet> faIds = new Dictionary<int, Facet>();

            try
            {
                var file = File.OpenRead(path);

                using (var reader = new StreamReader(file))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        if (parts[0].Equals("solid"))
                            continue;
                        if (parts[0].Equals("endsolid"))
                            break;

                        try
                        {
                            if (parts[0] == "facet" && parts[1] == "normal")
                            {
                                var normal = ParseVector3f(line, 2);
                                SkipLines(reader, 1);
                                var v0 = ParseVertex(reader.ReadLine(), veIds);
                                var v1 = ParseVertex(reader.ReadLine(), veIds);
                                var v2 = ParseVertex(reader.ReadLine(), veIds);

                                normal = Vector3.Normalize(Vector3.Cross(Vector3.Normalize(v1 - v0), Vector3.Normalize(v2 - v0)));

                                var facet = new Facet(veIds[v0], veIds[v1], veIds[v2], normal);

                                faIds.Add(faIds.Count, facet);
                            }
                        }
                        catch
                        {
                            System.Diagnostics.Debug.WriteLine("Failed to read ascii stl file!");
                            return null;
                        }
                    }
                }

                var vertexIdDict = BuildVertexDictionary(veIds);

                var vertexOccurrences = new Dictionary<int, int>();
                var normalsMapping = new Dictionary<int, Vector3>();

                foreach(var facet in faIds.Values)
                {
                    var v0 = facet.V0;
                    var v1 = facet.V1;
                    var v2 = facet.V2;

                    if (!vertexOccurrences.ContainsKey(v0))
                    {
                        vertexOccurrences[v0] = 0;
                        normalsMapping[v0] = Vector3.Zero;
                    }
                    vertexOccurrences[v0]++;
                    normalsMapping[v0] += facet.Normal;

                    if (!vertexOccurrences.ContainsKey(v1))
                    {
                        vertexOccurrences[v1] = 0;
                        normalsMapping[v1] = Vector3.Zero;
                    }
                    vertexOccurrences[v1]++;
                    normalsMapping[v1] += facet.Normal;

                    if (!vertexOccurrences.ContainsKey(v2))
                    {
                        vertexOccurrences[v2] = 0;
                        normalsMapping[v2] = Vector3.Zero;
                    }
                    vertexOccurrences[v2]++;
                    normalsMapping[v2] += facet.Normal;
                }

                foreach(var veId in vertexIdDict.Keys)
                {
                    normalsMapping[veId] /= vertexOccurrences[veId];
                    vertexIdDict[veId].SetNormal(normalsMapping[veId]);
                }

                return new Mesh<IVertex>(vertexIdDict, faIds);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        private Dictionary<int, IVertex> BuildVertexDictionary(Dictionary<Vector3, int> veIds)
        {
            var result = new Dictionary<int, IVertex>();

            var index = 0;

            var maxLength = 0.0f;
            foreach(var key in  veIds.Keys)
            {
                var length = key.Length();
                if (length > maxLength)
                {
                    maxLength = length;
                }
            }

            foreach (var veId in veIds)
            {
                var position = veId.Key / maxLength;
                result.Add(index++, new StandardVertex(position));
            }

            return result;
        }

        private Vector3 ParseVertex(string line, Dictionary<Vector3, int> vertexIdDictionary)
        {
            var vertex = ParseVector3f(line, 1);

            if(!vertexIdDictionary.ContainsKey(vertex))
                vertexIdDictionary.Add(vertex, vertexIdDictionary.Count);

            return vertex;
        }

        private Vector3 ParseVector3f(string line, int skip)
        {
            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var x = float.Parse(parts[skip + 0]);
            var y = float.Parse(parts[skip + 1]);
            var z = float.Parse(parts[skip + 2]);

            return new Vector3(x, y, z);
        }

        private void SkipLines(StreamReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                reader.ReadLine();
            }
        }
    }
}
