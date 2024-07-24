using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.FrameBuffers;
using System;
using System.Drawing;
using g3;
using System.Linq;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Collections.Generic;
using System.Numerics;
using SoftwareRenderer3D.Rasterizers;
using System.Threading.Tasks;
using SoftwareRenderer3D.Utils;

namespace SoftwareRenderer3D.Renderers
{
    public static class SubsurfaceScatteringRenderer
    {
        private static Mesh<IVertex> _lastRenderedMesh;
        private static Dictionary<int, double> _lastRenderedMeshSubsurfaceScatteringMapping;
        public static Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            if (mesh == null)
                return frameBuffer.GetFrame();

            if (_lastRenderedMesh == null || !_lastRenderedMesh.Equals(mesh))
            {
                _lastRenderedMesh = mesh;
                _lastRenderedMeshSubsurfaceScatteringMapping = CalculateSubsurfaceScattering(mesh);
            }
            _lastRenderedMesh.Equals(mesh);

            TexturedScanLineRasterizer.BindTexture(texture);

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;

            Matrix4x4.Invert(mesh.ModelMatrix, out var modelMatrix);

            var vertices = new Dictionary<int, IVertex>(mesh.VertexCount);

            var vertexIds = mesh.VertexIds;

            Parallel.ForEach(vertexIds, vertexId =>
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

            var lightSources = new List<Vector3>()
            {
                new Vector3(0, 100, 100),
                new Vector3(0, -123, -242),
            };

            var facetIds = Globals.BackfaceCulling
                ? mesh.FacetIds.Where(faId => Vector3.Dot((mesh.GetFacetMidpoint(faId) - camera.EyePosition).Normalize(), mesh.GetFacetNormal(faId)) <= 0.1)
                : mesh.FacetIds;

            Parallel.ForEach(facetIds, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, facetId =>
            {
                var facet = mesh.GetFacet(facetId);

                var v0 = mesh.GetVertex(facet.V0);
                var v1 = mesh.GetVertex(facet.V1);
                var v2 = mesh.GetVertex(facet.V2);

                var normal = facet.Normal;

                var lightContribution = 0.0f;
                foreach (var lightSource in lightSources)
                {
                    var lightDir = (lightSource - mesh.GetFacetNormal(facetId)).Normalize();
                    lightContribution += MathUtils.Clamp(Vector3.Dot(lightDir, normal.Normalize()).Clamp(), 0, 1);
                }

                lightContribution = lightContribution.Clamp(0, 1);

                if (RenderUtils.IsTriangleInFrustum(width, height, v0.ScreenPosition, v1.ScreenPosition, v2.ScreenPosition))
                {
                    if (mesh.GetVertex(facet.V0).GetType().IsAssignableFrom(typeof(TexturedVertex)))
                        TexturedScanLineRasterizer.ScanLineTriangle(frameBuffer, v0 as TexturedVertex, v1 as TexturedVertex, v2 as TexturedVertex, lightSources);
                    else
                        ScanLineRasterizer.ScanLineTriangle(frameBuffer, v0, v1, v2, lightSources);
                }
            });

            TexturedScanLineRasterizer.UnbindTexture();

            return frameBuffer.GetFrame();
        }

        private static Dictionary<int, double> CalculateSubsurfaceScattering(Mesh<IVertex> mesh)
        {
            var subsurfaceDistanceTraveled = new Dictionary<int, double>();

            var vertexIds = mesh.VertexIds;
            var facets = mesh.Facets;

            var g3Vertices = vertexIds.Select(veId => new Vector3f(mesh.GetVertexPoint(veId).ToVector3f())).ToList();


            DMesh3 g3Mesh = new DMesh3(MeshComponents.VertexNormals);
            for (int i = 0; i < vertexIds.Count(); ++i)
                g3Mesh.AppendVertex(new NewVertexInfo(g3Vertices[i]));
            foreach (var facet in facets)
                g3Mesh.AppendTriangle(new Index3i(facet.V0, facet.V1, facet.V2));

            DMeshAABBTree3 spatial = new DMeshAABBTree3(g3Mesh);
            spatial.Build();

            Parallel.ForEach(vertexIds, new ParallelOptions { MaxDegreeOfParallelism = 10 }, vertexId =>
            {
                var containsKey = false;

                lock (subsurfaceDistanceTraveled)
                {
                    if (!subsurfaceDistanceTraveled.ContainsKey(vertexId))
                        subsurfaceDistanceTraveled.Add(vertexId, -1);
                }

                if (!containsKey)
                {
                    var subsurfaceDistance = CalculateVertexSubsurfaceDistanceTraveled(mesh, subsurfaceDistanceTraveled, g3Mesh, spatial, vertexId);
                    lock (subsurfaceDistanceTraveled)
                    {
                        subsurfaceDistanceTraveled[vertexId] = subsurfaceDistance;
                    }
                }

            });

            var maxDistanceTraveled = subsurfaceDistanceTraveled.Values.Max();
            var subsurfaceScatteringAmounts = new Dictionary<int, double>(subsurfaceDistanceTraveled.Count);

            foreach (var keyValue in subsurfaceDistanceTraveled)
            {
                var normalizedDistance = keyValue.Value / maxDistanceTraveled;
                subsurfaceScatteringAmounts[keyValue.Key] = CalculateLightDecay(normalizedDistance);
            }

            return subsurfaceScatteringAmounts;
        }

        private static double CalculateVertexSubsurfaceDistanceTraveled(Mesh<IVertex> mesh, Dictionary<int, double> lightContribution, DMesh3 g3Mesh, DMeshAABBTree3 spatial, int vertexId)
        {
            var origin = new Vector3(0, 100, 100).ToVector3d();
            var vertexPos = mesh.GetVertex(vertexId).WorldPoint.ToVector3d();
            var direction = (vertexPos - origin).Normalized;

            Ray3d ray = new Ray3d(origin, direction);
            int hit_tid = spatial.FindNearestHitTriangle(ray);

            if (hit_tid != DMesh3.InvalidID)
            {
                IntrRay3Triangle3 intr = MeshQueries.TriangleIntersection(g3Mesh, hit_tid, ray);
                double hit_dist = origin.Distance(ray.PointAt(intr.RayParameter));

                var distanceToVertex = origin.Distance(vertexPos) - hit_dist;

                return distanceToVertex;
            }

            return 0;
        }

        private static double CalculateLightDecay(double distance)
        {
            return System.Math.Pow(System.Math.E, -2.0 * distance);
        }
    }
}
