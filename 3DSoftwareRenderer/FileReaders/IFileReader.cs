using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;

namespace SoftwareRenderer3D.FileReaders
{
    public interface IFileReader
    {
        Mesh<IVertex> ReadFile(string path);
    }
}
