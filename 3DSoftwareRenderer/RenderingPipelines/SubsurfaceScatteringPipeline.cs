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
using SoftwareRenderer3D.FragmentShaders;

namespace SoftwareRenderer3D.RenderingPipelines
{
    public class SubsurfaceScatteringPipeline : IRenderPipeline
    {
        private static Mesh<IVertex> _lastRenderedMesh;
        private static Dictionary<IVertex, double> _subsurfaceScatteringAmount;
        private const double LightDecayParameter = 0.2;

        public Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
        {
            if (mesh == null)
                return frameBuffer.GetFrame();

            if (_lastRenderedMesh == null || !_lastRenderedMesh.Equals(mesh))
            {
                _lastRenderedMesh = mesh;
                _subsurfaceScatteringAmount = CalculateSubsurfaceScattering(mesh);
            }
            _lastRenderedMesh.Equals(mesh);

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;
            var modelMatrix = mesh.ModelMatrix;

            mesh.TransformVertices(width, height, viewMatrix, projectionMatrix);

            var facetIds = Globals.BackfaceCulling
                ? mesh.FacetIds.Where(faId => Vector3.Dot((mesh.GetFacetMidpoint(faId) - camera.EyePosition).Normalize(), mesh.GetFacetNormal(faId)) <= 0.1)
                : mesh.FacetIds;

            var fragments = ScanLineRasterizer.Rasterize(mesh, width, height, facetIds);

            var lightSources = Globals.LightSources;

            SubsurfaceScatteringFragmentShader.BindTexture(texture);
            SubsurfaceScatteringFragmentShader.ShadeFragments(frameBuffer, lightSources, fragments, _subsurfaceScatteringAmount);
            SubsurfaceScatteringFragmentShader.UnbindTexture();

            return frameBuffer.GetFrame();
        }

        private static Dictionary<IVertex, double> CalculateSubsurfaceScattering(Mesh<IVertex> mesh)
        {
            var subsurfaceDistanceTraveled = new double[mesh.VertexCount];

            var vertexIds = mesh.VertexIds;
            var facets = mesh.Facets;

            var g3Vertices = vertexIds.Select(veId => new Vector3f(mesh.GetVertexPoint(veId).ToVector3f())).ToList();

            DMesh3 g3Mesh = new DMesh3();
            for (int i = 0; i < vertexIds.Count(); ++i)
                g3Mesh.AppendVertex(new NewVertexInfo(g3Vertices[i]));
            foreach (var facet in facets)
                g3Mesh.AppendTriangle(new Index3i(facet.V0, facet.V1, facet.V2));

            DMeshAABBTree3 spatial = new DMeshAABBTree3(g3Mesh);
            spatial.Build();

            Parallel.ForEach(vertexIds, new ParallelOptions { MaxDegreeOfParallelism = Constants.NumberOfThreads }, vertexId =>
            {
                var vertex = mesh.GetVertex(vertexId);
                var subsurfaceDistance = CalculateVertexSubsurfaceDistanceTraveled(mesh, g3Mesh, spatial, vertexId);
                subsurfaceDistanceTraveled[vertexId] = CalculateLightDecay(subsurfaceDistance);
            });

            var maxDistanceTraveled = subsurfaceDistanceTraveled.Max();
            var subsurfaceScatteringAmounts = new Dictionary<IVertex, double>(subsurfaceDistanceTraveled.Length);

            var maxDistance = subsurfaceDistanceTraveled.Max();
            for (var vertexId = 0; vertexId < subsurfaceDistanceTraveled.Length; vertexId++)
            {
                var distance = subsurfaceDistanceTraveled[vertexId];
                var normalizedDistance = distance / maxDistance;
                subsurfaceDistanceTraveled[vertexId] = 1 - normalizedDistance;
            }

            for (var vertexId = 0; vertexId < subsurfaceDistanceTraveled.Length; vertexId++)
            {
                var distance = subsurfaceDistanceTraveled[vertexId];
                var vertex = mesh.GetVertex(vertexId);
                subsurfaceScatteringAmounts[vertex] = distance;
            }

            return subsurfaceScatteringAmounts;
        }

        private static double CalculateVertexSubsurfaceDistanceTraveled(Mesh<IVertex> mesh, DMesh3 g3Mesh, DMeshAABBTree3 spatial, int vertexId)
        {
            var origin = Globals.LightSources.First().ToVector3d();
            var vertexPos = mesh.GetVertex(vertexId).Position.ToVector3d();
            var direction = (origin - vertexPos).Normalized;

            Ray3d ray = new Ray3d(vertexPos + 0.01 * direction, direction);
            int hitTriangleId = spatial.FindNearestHitTriangle(ray);

            if (hitTriangleId != DMesh3.InvalidID)
            {
                IntrRay3Triangle3 intersection = MeshQueries.TriangleIntersection(g3Mesh, hitTriangleId, ray);
                double hitDistance = vertexPos.Distance(ray.PointAt(intersection.RayParameter));

                return hitDistance;
            }

            return 0;
        }

        private static double CalculateLightDecay(double distance)
        {
            return distance;
            return Math.Exp(1 / (1 + LightDecayParameter * distance));
        }
    }
}
