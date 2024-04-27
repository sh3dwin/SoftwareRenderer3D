using SoftwareRenderer3D.Enums;
using SoftwareRenderer3D.FileReaders;
using System;
using System.Collections.Generic;

namespace SoftwareRenderer3D.Factories
{
    public static class FileReaderFactory
    {
        private static Dictionary<FileType, IFileReader> _factory = new Dictionary<FileType, IFileReader>()
        {
            {FileType.STL, new STLReader()},
            {FileType.Collada, new ColladaReader()},
        };

        public static IFileReader GetFileReader(string filename)
        {
            if (filename.EndsWith(".stl"))
                return _factory[FileType.STL];
            if (filename.EndsWith(".dae"))
                return _factory[FileType.Collada];

            throw new Exception($"The {filename.Substring(filename.Length - 4)} format is not supported!");
        }
    }
}
