using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using System;
using System.IO;
using System.Collections.Generic;
using g3;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;

namespace SoftwareRenderer3D.FileReaders
{
    public class STLReader : IFileReader
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

            Dictionary<Vector3f, int> veIds = new Dictionary<Vector3f, int>();
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

                return new Mesh<IVertex>(vertexIdDict, faIds);

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }

            return null;
        }

        private Dictionary<int, IVertex> BuildVertexDictionary(Dictionary<Vector3f, int> veIds)
        {
            var result = new Dictionary<int, IVertex>();

            var index = 0;
            foreach (var veId in veIds)
            {
                result.Add(index++, new StandardVertex(veId.Key));
            }

            return result;
        }

        private Vector3f ParseVertex(string line, Dictionary<Vector3f, int> vertexIdDictionary)
        {
            var vertex = ParseVector3f(line, 1);

            if(!vertexIdDictionary.ContainsKey(vertex))
                vertexIdDictionary.Add(vertex, vertexIdDictionary.Count);

            return vertex;
        }

        private Vector3f ParseVector3f(string line, int skip)
        {
            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var x = float.Parse(parts[skip + 0]);
            var y = float.Parse(parts[skip + 1]);
            var z = float.Parse(parts[skip + 2]);

            return new Vector3f(x, y, z);
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
