using System.Numerics;

namespace SoftwareRenderer3D.DataStructures.FacetDataStructures
{
    public class Facet
    {
        private int _vertex0;
        private int _vertex1;
        private int _vertex2;

        private Vector3 _normal;

        public Facet(int vertex0, int vertex1, int vertex2, Vector3 normal)
        {
            _vertex0 = vertex0;
            _vertex1 = vertex1;
            _vertex2 = vertex2;
            _normal = normal;
        }

        public int V0 => _vertex0;
        public int V1 => _vertex1;
        public int V2 => _vertex2;
        public Vector3 Normal => _normal;
    }
}
