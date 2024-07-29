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
    public static class SubsurfaceScatteringPipeline
    {
        private static Mesh<IVertex> _lastRenderedMesh;
        private static Dictionary<IVertex, double> _subsurfaceScatteringAmount;
        public static Bitmap Render(Mesh<IVertex> mesh, IFrameBuffer frameBuffer, ArcBallCamera camera, Texture texture = null)
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
            var subsurfaceDistanceTraveled = new Dictionary<IVertex, double>();

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

            Parallel.ForEach(vertexIds, vertexId =>
            {
                var vertex = mesh.GetVertex(vertexId);
                var subsurfaceDistance = CalculateVertexSubsurfaceDistanceTraveled(mesh, g3Mesh, spatial, vertexId);
                lock (subsurfaceDistanceTraveled)
                {
                    subsurfaceDistanceTraveled[vertex] = subsurfaceDistance;
                }
            });

            var maxDistanceTraveled = subsurfaceDistanceTraveled.Values.Max();
            var subsurfaceScatteringAmounts = new Dictionary<IVertex, double>(subsurfaceDistanceTraveled.Count);

            foreach (var keyValue in subsurfaceDistanceTraveled)
            {
                var normalizedDistance = keyValue.Value / maxDistanceTraveled;
                subsurfaceScatteringAmounts[keyValue.Key] = CalculateLightDecay(normalizedDistance);
            }

            return subsurfaceScatteringAmounts;
        }

        private static double CalculateVertexSubsurfaceDistanceTraveled(Mesh<IVertex> mesh, DMesh3 g3Mesh, DMeshAABBTree3 spatial, int vertexId)
        {
            var origin = Globals.LightSources.First().ToVector3d();
            var vertexPos = mesh.GetVertex(vertexId).WorldPoint.ToVector3d();
            var direction = (vertexPos - origin).Normalized;

            Ray3d ray = new Ray3d(origin, direction);
            int hitTriangleId = spatial.FindNearestHitTriangle(ray);

            if (hitTriangleId != DMesh3.InvalidID)
            {
                IntrRay3Triangle3 intersection = MeshQueries.TriangleIntersection(g3Mesh, hitTriangleId, ray);
                double hitDistance = origin.Distance(ray.PointAt(intersection.RayParameter));

                var distanceToVertex = origin.Distance(vertexPos) - hitDistance;

                return distanceToVertex;
            }

            return 0;
        }

        private static double CalculateLightDecay(double distance)
        {
            return Math.Pow(Math.E, -1.0 * distance);
        }
    }
}
