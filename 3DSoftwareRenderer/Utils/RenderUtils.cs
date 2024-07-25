using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.Utils
{
    public static class RenderUtils
    {
        public static bool InInclusiveLowerExclusiveUpper(float x, int lower, int upper)
        {
            return x >= lower && x < upper;
        }

        public static bool IsTriangleInFrustum(int width, int height, Vector3 v0, Vector3 v1, Vector3 v2)
        {
            return InInclusiveLowerExclusiveUpper(v0.X, 0, width) && InInclusiveLowerExclusiveUpper(v0.Y, 0, height) ||
                InInclusiveLowerExclusiveUpper(v1.X, 0, width) && InInclusiveLowerExclusiveUpper(v1.Y, 0, height) ||
                InInclusiveLowerExclusiveUpper(v2.X, 0, width) && InInclusiveLowerExclusiveUpper(v2.Y, 0, height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HaveClockwiseOrientation(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return Cross2D(p0, p1, p2) > 0;
        }

        // https://www.geeksforgeeks.org/orientation-3-ordered-points/

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cross2D(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return (p1.X - p0.X) * (p2.Y - p1.Y) - (p1.Y - p0.Y) * (p2.X - p1.X);
        }

        public static (IVertex i0, IVertex i1, IVertex i2) SortIndices(IVertex p0, IVertex p1, IVertex p2)
        {
            var c0 = p0.ScreenPosition.Y;
            var c1 = p1.ScreenPosition.Y;
            var c2 = p2.ScreenPosition.Y;

            if (c0 < c1)
            {
                if (c2 < c0)
                    return (p2, p0, p1);
                if (c1 < c2)
                    return (p0, p1, p2);
                return (p0, p2, p1);
            }

            if (c2 < c1)
                return (p2, p1, p0);
            if (c0 < c2)
                return (p1, p0, p2);
            return (p1, p2, p0);

        }

        public static void TransformVertices(Mesh<IVertex> mesh, int width, int height, Matrix4x4 viewMatrix, Matrix4x4 projectionMatrix, Matrix4x4 modelMatrix, Dictionary<int, IVertex> vertices, IEnumerable<int> vertexIds)
        {
            Parallel.ForEach(vertexIds, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, vertexId =>
            {
                var vertex = mesh.GetVertex(vertexId);
                var modelV0 = vertex.WorldPoint.TransformHomogeneus(modelMatrix);
                modelV0 /= modelV0.W;

                var viewV0 = modelV0.Transform(viewMatrix);
                viewV0 /= viewV0.W;

                var clipV0 = viewV0.Transform(projectionMatrix);
                var ndcV0 = clipV0 / clipV0.W;

                vertex.NDCPosition = ndcV0.ToVector3();
                vertex.SetScreenCoordinates(width, height);
                vertices[vertexId] = vertex;
            });
        }
    }
}
