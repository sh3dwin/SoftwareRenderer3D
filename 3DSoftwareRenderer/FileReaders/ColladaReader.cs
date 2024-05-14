using g3;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.FacetDataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Linq;
using static g3.DPolyLine2f;

namespace SoftwareRenderer3D.FileReaders
{
    /// <summary>
    /// Source: https://github.com/larsjarlvik/collada-parser/tree/master
    /// Mostly the same, adapted for the relevant mesh data structure
    /// </summary>
    public class ColladaReader : IMeshFileReader
    {
        public Mesh<IVertex> ReadFile(string path)
        {
            throw new NotImplementedException();
        }

        private static XNamespace ns = "http://www.collada.org/2005/11/COLLADASchema";

        // Collada import is a brittle hack and need a serious work

       

        // Work in progress

        public static Mesh<IVertex> ImportCollada(string fileName)
        {
            XNamespace ns = "http://www.collada.org/2005/11/COLLADASchema";

            var xdoc = XDocument.Load(fileName);

            var geometries = xdoc.Root.Element(ns + "library_geometries").Elements(ns + "geometry");

            foreach (var geometry in geometries)
            {
                var mesh = geometry.Element(ns + "mesh");

                var polylist = mesh.Element(ns + "polylist");
                if (polylist != null)
                {
                    var polylist_vcount = parseArray<int>(polylist.Element(ns + "vcount")?.Value);

                    if (!polylist_vcount.Any() || polylist_vcount.Distinct().Any(v => v != 3))
                        throw new Exception();

                    var indices = parseArray<int>(polylist.Element(ns + "p")?.Value);
                    getSource(polylist, "VERTEX", out var polylist_vertex_id, out _);

                    var vertices = mesh.Elements(ns + "vertices").FirstOrDefault(e => e.Attribute("id")?.Value == polylist_vertex_id);
                    getSource(vertices, "POSITION", out var vertices_position_id, out _);
                    getSource(vertices, "NORMAL", out var vertices_normal_id, out _);
                    getSource(vertices, "TEXCOORD", out var texture_coordinates_id, out _);


                    var vertices_position = getArraySource<Vector3>(mesh, vertices_position_id);
                    var vertices_normal = getArraySource<Vector3>(mesh, vertices_normal_id);
                    var texture_coordinates = getArraySource<Vector2>(mesh, texture_coordinates_id);
                }
            }
        }

        static IEnumerable<Facet> GetFacets(int[] array, IEnumerable<Vector3> normals, int stride, int offset = 0)
        {
            var l = array.Length / stride;
            for (var i = 0; i < l; i++)
                yield return new Facet(array[i * stride + offset], array[i * stride + 4 + offset], array[i * stride + 8 + offset], normals.ElementAt(i));
        }

        static IEnumerable<T> parseArray<T>(string value)
        {
            return value?.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries)?.Select(v => (T)Convert.ChangeType(v, typeof(T), CultureInfo.InvariantCulture)) ?? Enumerable.Empty<T>();
        }

        static void getSource(XElement element, string semantic, out string id, out int offset)
        {
            var e = element?.Elements(element?.GetDefaultNamespace() + "input")?.FirstOrDefault(e => string.Equals(e.Attribute("semantic")?.Value, semantic));
            id = e?.Attribute("source")?.Value?.TrimStart('#');
            offset = int.Parse(e?.Attribute("offset")?.Value ?? "0");
        }

        static IEnumerable<T> getArraySource<T>(XElement mesh, string id)
        {
            var ns = mesh.GetDefaultNamespace();

            var data = mesh
                .Elements(ns + "source")
                .FirstOrDefault(e => e.Attribute("id").Value == id)
                .Element(ns + "float_array")
                .Value;

            var floats = parseArray<float>(data).ToArray();

            if (typeof(T) == typeof(Vector3))
            {
                for (var i = 0; i < floats.Length; i += 3)
                    yield return (T)(object)new Vector3(floats[i], floats[i + 1], floats[i + 2]);
            }

            if (typeof(T) == typeof(Vector2))
            {
                for (var i = 0; i < floats.Length; i += 2)
                    yield return (T)(object)new Vector2(floats[i], floats[i + 1]);
            }

        }

        public static void RemoveDuplicates(IEnumerable<Vector3> vertices, IEnumerable<Vector2> textures, IEnumerable<Vector3> normals,) { }
    }
}
