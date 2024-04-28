using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;

namespace SoftwareRenderer3D.FileReaders
{
    public interface IMeshFileReader
    {
        Mesh<IVertex> ReadFile(string path);
    }
}
