using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3DSoftwareRenderer.DataStructures.FacetDataStructures
{
    public class Facet
    {
        private int _vertex0;
        private int _vertex1;
        private int _vertex2;

        public Facet(int vertex0, int vertex1, int vertex2)
        {
            _vertex0 = vertex0;
            _vertex1 = vertex1;
            _vertex2 = vertex2;
        }

        public int V0 => _vertex0;
        public int V1 => _vertex1;
        public int V2 => _vertex2;
    }
}
