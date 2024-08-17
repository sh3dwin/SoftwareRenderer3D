using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using System;
using System.IO;
using System.Collections.Generic;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using System.Numerics;
using g3;
using System.Windows.Shapes;

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

                using (var reader = new StreamReader(file))
                {
                    var firstLine = reader.ReadLine();

                    if (firstLine.StartsWith("solid"))
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

        private Mesh<IVertex> ReadBinary(string filePath)
        {
            bool processError = false;

            Dictionary<Vector3, int> veIds = new Dictionary<Vector3, int>();
            Dictionary<int, Facet> faIds = new Dictionary<int, Facet>();

            var fileBytes = File.ReadAllBytes(filePath);

            var temp = new byte[4];

            /* 80 bytes title + 4 byte num of triangles + 50 bytes (1 of triangular mesh)  */
            if (fileBytes.Length > 120)
            {
                temp[0] = fileBytes[80];
                temp[1] = fileBytes[81];
                temp[2] = fileBytes[82];
                temp[3] = fileBytes[83];

                var numOfMesh = BitConverter.ToInt32(temp, 0);

                var byteIndex = 84;

                for (var i = 0; i < numOfMesh; i++)
                {
                    /* this try-catch block will be reviewed */
                    try
                    {
                        /* face normal */
                        var normal = ParseVector3Binary(fileBytes, byteIndex);
                        byteIndex += 3 * sizeof(float);
                        normal = Vector3.Normalize(normal);

                        /* vertex 1 */
                        var v0 = ParseVector3Binary(fileBytes, byteIndex);
                        byteIndex += 3 * sizeof(float);
                        ProcessVertex(veIds, v0);


                        /* vertex 2 */
                        var v1 = ParseVector3Binary(fileBytes, byteIndex);
                        byteIndex += 3 * sizeof(float);
                        ProcessVertex(veIds, v1);

                        /* vertex 3 */
                        var v2 = ParseVector3Binary(fileBytes, byteIndex);
                        byteIndex += 3 * sizeof(float);
                        ProcessVertex(veIds, v2);

                        normal = Vector3.Normalize(Vector3.Cross(Vector3.Normalize(v1 - v0), Vector3.Normalize(v2 - v0)));
                        var facet = new Facet(veIds[v0], veIds[v1], veIds[v2], normal);

                        faIds.Add(faIds.Count, facet);

                        byteIndex += 2; // Attribute byte count
                    }
                    catch
                    {
                        processError = true;
                        break;
                    }
                }
            }

            if (processError)
            {
                throw new FileLoadException($"Error reading BINARY STL file: {filePath}");
            }

            var vertexIdDict = BuildVertexDictionary(veIds);

            return new Mesh<IVertex>(vertexIdDict, faIds);
        }

        private Mesh<IVertex> ReadASCII(string path)
        {

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
                                var normal = ParseVector3ASCII(line, 2);

                                SkipLines(reader, 1);
                                var v0 = ParseVector3ASCII(reader.ReadLine(), 1);
                                ProcessVertex(veIds, v0);
                                var v1 = ParseVector3ASCII(reader.ReadLine(), 1);
                                ProcessVertex(veIds, v1);
                                var v2 = ParseVector3ASCII(reader.ReadLine(), 1);
                                ProcessVertex(veIds, v2);

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
            foreach (var key in veIds.Keys)
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

        private Vector3 ProcessVertex(Dictionary<Vector3, int> vertexIdDictionary, Vector3 vertex)
        {
            if (!vertexIdDictionary.ContainsKey(vertex))
                vertexIdDictionary.Add(vertex, vertexIdDictionary.Count);

            return vertex;
        }

        private Vector3 ParseVector3ASCII(string line, int skip)
        {
            var parts = line.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var x = float.Parse(parts[skip + 0]);
            var y = float.Parse(parts[skip + 1]);
            var z = float.Parse(parts[skip + 2]);

            return new Vector3(x, y, z);
        }

        private static Vector3 ParseVector3Binary(byte[] fileBytes, int byteIndex)
        {
            var x = BitConverter.ToSingle(
                                        new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                                        }, 0);
            byteIndex += 4;
            var y = BitConverter.ToSingle(
                new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                }, 0);
            byteIndex += 4;
            var z = BitConverter.ToSingle(
                new[] {
                                fileBytes[byteIndex], fileBytes[byteIndex + 1], fileBytes[byteIndex + 2],
                                fileBytes[byteIndex + 3]
                }, 0);

            var result = new Vector3(x, y, z);

            return result;
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
