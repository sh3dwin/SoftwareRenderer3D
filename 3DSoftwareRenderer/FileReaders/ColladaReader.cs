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
            return Collada.ColladaLoader.Load(path).Geometries.First();
        }

    }
}
