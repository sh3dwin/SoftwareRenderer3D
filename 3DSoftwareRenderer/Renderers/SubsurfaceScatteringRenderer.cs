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
        public static Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            var lightContributions = CalculateSubsurfaceScattering(mesh);

            TexturedScanLineRasterizer.BindTexture(texture);

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;

            Matrix4x4.Invert(mesh.ModelMatrix, out var modelMatrix);

            var lightSourceAt = new Vector3(0, 100, 100);

            var facets = mesh.GetFacets().Where((x, i) =>
            Vector3.Dot(
                (mesh.GetFacetMidpoint(i) - camera.Position).Normalize(),
                x.Normal.Normalize()) <= 0.3);

            Parallel.ForEach(facets, new ParallelOptions() { MaxDegreeOfParallelism = 1 }, facet =>
            {
                var v0 = mesh.GetVertexPoint(facet.V0);
                var v1 = mesh.GetVertexPoint(facet.V1);
                var v2 = mesh.GetVertexPoint(facet.V2);

                var normal = facet.Normal;

                var lightContribution = MathUtils.Clamp(-Vector3.Dot(lightSourceAt.Normalize(), normal.Normalize()) + (float)lightContributions[facet.V0], 0, 1);
                lightContribution = (float)lightContributions[facet.V0].Clamp();

                var modelV0 = v0.TransformHomogeneus(modelMatrix);
                modelV0 /= modelV0.W;
                var modelV1 = v1.TransformHomogeneus(modelMatrix);
                modelV1 /= modelV1.W;
                var modelV2 = v2.TransformHomogeneus(modelMatrix);
                modelV2 /= modelV2.W;

                var viewV0 = modelV0.Transform(viewMatrix);
                viewV0 /= viewV0.W;
                var viewV1 = modelV1.Transform(viewMatrix);
                viewV1 /= viewV1.W;
                var viewV2 = modelV2.Transform(viewMatrix);
                viewV2 /= viewV2.W;

                var clipV0 = viewV0.Transform(projectionMatrix);
                var ndcV0 = clipV0 / clipV0.W;
                var clipV1 = viewV1.Transform(projectionMatrix);
                var ndcV1 = clipV1 / clipV1.W;
                var clipV2 = viewV2.Transform(projectionMatrix);
                var ndcV2 = clipV2 / clipV2.W;

                var screenV0 = new Vector3((ndcV0.X + 1) * width / 2.0f, (-ndcV0.Y + 1) * height / 2.0f, ndcV0.Z);
                var screenV1 = new Vector3((ndcV1.X + 1) * width / 2.0f, (-ndcV1.Y + 1) * height / 2.0f, ndcV1.Z);
                var screenV2 = new Vector3((ndcV2.X + 1) * width / 2.0f, (-ndcV2.Y + 1) * height / 2.0f, ndcV2.Z);

                if (RendererUtils.IsTriangleWithinScreen(width, height, screenV0, screenV1, screenV2))
                {
                    if (mesh.GetVertex(facet.V0).GetType().IsAssignableFrom(typeof(TexturedVertex)))
                    {
                        TexturedScanLineRasterizer.ScanLineTriangle(frameBuffer, screenV0, screenV1, screenV2,
                            mesh.GetVertex(facet.V0) as TexturedVertex, mesh.GetVertex(facet.V1) as TexturedVertex, mesh.GetVertex(facet.V2) as TexturedVertex, lightContribution);
                    }
                    else
                    {
                        ScanLineRasterizer.ScanLineTriangle(frameBuffer, screenV0, screenV1, screenV2, lightContribution);
                    }
                }
            });

            TexturedScanLineRasterizer.UnbindTexture();

            return frameBuffer.GetFrame();
        }

        private static Dictionary<int, double> CalculateSubsurfaceScattering(Mesh<IVertex> mesh)
        {
            var subsurfaceDistanceTraveled = new Dictionary<int, double>();

            var vertices = mesh.Vertices;
            var facets = mesh.Facets;

            var g3Vertices = vertices.Select(vertex => new Vector3f(vertex.GetVertexPoint().ToVector3f())).ToList();


            DMesh3 g3Mesh = new DMesh3(MeshComponents.VertexNormals);
            for (int i = 0; i < vertices.Count(); ++i)
                g3Mesh.AppendVertex(new NewVertexInfo(g3Vertices[i]));
            foreach (var facet in facets)
                g3Mesh.AppendTriangle(new Index3i(facet.V0, facet.V1, facet.V2));

            DMeshAABBTree3 spatial = new DMeshAABBTree3(g3Mesh);
            spatial.Build();

            Parallel.ForEach(facets, new ParallelOptions {  MaxDegreeOfParallelism = 1} ,facet =>
            {
                var vertexIds = new int[3] { facet.V0, facet.V1, facet.V2 };

                foreach (var vertexId in vertexIds)
                {
                    if(vertexId == 1074)
                    {

                    }
                    var containsKey = false;

                    lock (subsurfaceDistanceTraveled) 
                    {
                         if (!subsurfaceDistanceTraveled.ContainsKey(vertexId))
                            subsurfaceDistanceTraveled.Add(vertexId, -1);
                    }

                    if (!containsKey)
                    {
                        var subsurfaceDistance = CalculateVertexSubsurfaceDistanceTraveled(mesh, subsurfaceDistanceTraveled, g3Mesh, spatial, vertexId);
                        if(subsurfaceDistance < 0)
                        {

                        }
                        lock (subsurfaceDistanceTraveled)
                        {
                            subsurfaceDistanceTraveled[vertexId] = subsurfaceDistance;
                        }
                    }
                }
            });

            var maxDistanceTraveled = subsurfaceDistanceTraveled.Values.Max();
            var subsurfaceScatteringAmounts = new Dictionary<int, double>(subsurfaceDistanceTraveled.Count);

            foreach(var keyValue in subsurfaceDistanceTraveled)
            {
                var normalizedDistance = keyValue.Value / maxDistanceTraveled;
                subsurfaceScatteringAmounts[keyValue.Key] = CalculateLightDecay(normalizedDistance);
            }

            return subsurfaceScatteringAmounts;
        }

        private static double CalculateVertexSubsurfaceDistanceTraveled(Mesh<IVertex> mesh, Dictionary<int, double> lightContribution, DMesh3 g3Mesh, DMeshAABBTree3 spatial, int vertexId)
        {
            var origin = new Vector3(0, 100, 100).ToVector3d();
            var vertexPos = mesh.GetVertex(vertexId).GetVertexPoint().ToVector3d();
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
            return Math.Pow(Math.E, -2.0 * distance);
        }
    }
}
