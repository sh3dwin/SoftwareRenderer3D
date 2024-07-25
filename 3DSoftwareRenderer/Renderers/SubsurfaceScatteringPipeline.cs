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

namespace SoftwareRenderer3D.Renderers
{
    public static class SubsurfaceScatteringPipeline
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

            TextureShader.BindTexture(texture);

            var width = frameBuffer.GetSize().Width;
            var height = frameBuffer.GetSize().Height;

            var viewMatrix = camera.ViewMatrix;
            var projectionMatrix = camera.ProjectionMatrix;
            var modelMatrix = mesh.ModelMatrix;

            mesh.TransformVertices(width, height, viewMatrix, projectionMatrix);

            var lightSources = Globals.LightSources;

            var facetIds = Globals.BackfaceCulling
                ? mesh.FacetIds.Where(faId => Vector3.Dot((mesh.GetFacetMidpoint(faId) - camera.EyePosition).Normalize(), mesh.GetFacetNormal(faId)) <= 0.1)
                : mesh.FacetIds;

            var fragments = ScanLineRasterizer.Rasterize(mesh, width, height, facetIds);

            if (mesh.Vertices.First().GetType().IsAssignableFrom(typeof(TexturedVertex)))
                TextureShader.ShadeFragments(lightSources, frameBuffer, fragments);
            else
                SimpleFragmentShader.ShadeFragments(frameBuffer, lightSources, fragments);

            TextureShader.UnbindTexture();

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
                    var subsurfaceDistance = CalculateVertexSubsurfaceDistanceTraveled(mesh, g3Mesh, spatial, vertexId);
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

        private static double CalculateVertexSubsurfaceDistanceTraveled(Mesh<IVertex> mesh, DMesh3 g3Mesh, DMeshAABBTree3 spatial, int vertexId)
        {
            var origin = new Vector3(0, 100, 100).ToVector3d();
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
            return Math.Pow(Math.E, -2.0 * distance);
        }
    }
}
