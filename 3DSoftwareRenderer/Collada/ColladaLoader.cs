using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace SoftwareRenderer3D.Collada
{
    /// <summary>
    /// Adapted from:
    /// https://github.com/larsjarlvik/collada-parser/tree/master
    /// </summary>
    public static class ColladaLoader
	{
		private static XNamespace ns = "{http://www.collada.org/2005/11/COLLADASchema}";

		public static ColladaMesh Load(string path)
		{
			using (var xml = new FileStream(path, FileMode.Open))
			{
				var xRoot = XDocument.Load(xml);
				var model = new ColladaMesh();

				// Parse Geometries
				var xMeshes = xRoot.Descendants($"{ns}mesh");
				if (!xMeshes.Any())
					throw new ApplicationException("Failed to find geometries!");

				foreach (var xMesh in xMeshes)
				{
					var geoLoader = new GeometryLoader(xMesh);
					var geometry = geoLoader.Load();

					model.Geometries.Add(geometry);
				}
				return model;
			}
		}
	}
}