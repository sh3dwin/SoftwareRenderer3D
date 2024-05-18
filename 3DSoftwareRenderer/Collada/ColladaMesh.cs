using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using System.Collections.Generic;

namespace SoftwareRenderer3D.Collada
{
    public class ColladaMesh
	{
		public List<Mesh<IVertex>> Geometries { get; set; }

		public ColladaMesh()
		{
			Geometries = new List<Mesh<IVertex>>();
		}
	}
}