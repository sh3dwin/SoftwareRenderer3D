using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Linq;

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
            return Collada.ColladaLoader.Load(path).Geometries.First();
        }

    }
}
