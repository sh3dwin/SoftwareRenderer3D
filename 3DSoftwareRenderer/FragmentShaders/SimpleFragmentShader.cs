using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.FrameBuffers;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.FragmentShaders
{
    public class SimpleFragmentShader
    {
        #region Benchmark
        public IEnumerable<IFragment> GetBenchmarkFragments()
        {
            var random = new Random(42);

            for (var i = 0; i < N; i++)
            {
                // Random ndc coords
                var x = random.NextDouble();
                var z = random.NextDouble();
                var y = random.NextDouble();

                var ndc = new Vector2((float)x, (float)y);

                // Random barycentric
                var bcX = random.NextDouble();
                var bcY = random.NextDouble();
                var bcZ = random.NextDouble();

                var denom = bcX + bcY + bcZ;
                bcX /= denom;
                bcY /= denom;
                bcZ /= denom;

                var barycentric = new Vector3((float)bcX, (float)bcY, (float)bcZ);

                // Random v0, v1, v2
                var vertices = new List<StandardVertex>(3);
                for (var j = 0; j < 3; j++)
                    vertices.Add(new StandardVertex(new Vector3((float)random.NextDouble(), (float)random.NextDouble(), (float)random.NextDouble())));

                yield return new SimpleFragment(ndc, (float)z, barycentric, vertices[0], vertices[1], vertices[2]);
            }
        }

        [ParamsSource(nameof(ValuesForLightSources))]
        public List<Vector3> LightSources;

        public List<IFragment> _benchmarkFragments;

        [Params(1_000, 10_000)]
        public int N;

        public IEnumerable<List<Vector3>> ValuesForLightSources => [new List<Vector3>() { new Vector3(0.0f, 0.0f, -1.0f) }];

        [GlobalSetup]
        public void GlobalSetup()
        {
            _benchmarkFragments = GetBenchmarkFragments().ToList();
        }

        [Benchmark]
        public int BenchmarkShaderFragment()
        {
            int result = default;

            for (int i = 0; i < _benchmarkFragments.Count; i++)
                result = ShadeFragment(_benchmarkFragments[i], LightSources);

            return result;
        }
        #endregion

        private Texture _texture = null;

        public void BindTexture(Texture texture)
        {
            _texture = texture;
        }

        public void UnbindTexture()
        {
            _texture = null;
        }

        public void ShadeFragments(IFrameBuffer frameBuffer, List<Vector3> lightSources, IReadOnlyList<IFragment> fragments)
        {
            var bucketSize = fragments.Count / Constants.NumberOfThreads;

            Parallel.For(0, Constants.NumberOfThreads, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, threadId =>
            {
                var startIndex = threadId * bucketSize;
                
                for (var i = startIndex; i < startIndex + bucketSize; i++)
                {
                    var fragment = fragments[i];
                    var argb = ShadeFragment(fragment, lightSources);
                    frameBuffer.SetPixelColor((int)fragment.ScreenCoordinates.X, (int)fragment.ScreenCoordinates.Y, (float)fragment.Depth, argb);
                }
            });
        }

        public int ShadeFragment(in IFragment fragment, List<Vector3> lightSources)
        {
            var diffuse = 0.0f;

            var opacity = Globals.NormalizedOpacity;

            Vector3 barycentricCoordinates = fragment.BarycentricCoordinates;
            var barX = barycentricCoordinates.X;
            var barY = barycentricCoordinates.Y;
            var barZ = barycentricCoordinates.Z;

            foreach (var lightSource in lightSources)
            {
                var interpolatedNormal = (fragment.V0.Normal * barX + fragment.V1.Normal * barY + fragment.V2.Normal * barZ);
                var worldPosition = fragment.V0.Position * barX + fragment.V1.Position * barY + fragment.V2.Position * barZ;
                var lightDirection = (worldPosition - lightSource).Normalize();

                var lightAngle = Vector3.Dot(interpolatedNormal, lightDirection);
                diffuse += (-lightAngle).Clamp(0, 1);
            }

            diffuse = diffuse.Clamp(0, 1);

            var v0FragmentColor = fragment.V0.Color;
            var v1FragmentColor = fragment.V1.Color;
            var v2FragmentColor = fragment.V2.Color;

            float r = v0FragmentColor.R * barX + v1FragmentColor.R * barY + v2FragmentColor.R * barZ;
            float g = v0FragmentColor.G * barX + v1FragmentColor.G * barY + v2FragmentColor.G * barZ;
            float b = v0FragmentColor.B * barX + v1FragmentColor.B * barY + v2FragmentColor.B * barZ;

            // Ensure within [0, 255]
            if (r > 255)
                r = 255;
            else if (r < 0)
                r = 0;

            if (g > 255)
                g = 255;
            else if (g < 0)
                g = 0;

            if (b > 255)
                b = 255;
            else if (b < 0)
                b = 0;

            return ((byte)(opacity * 255) << 24 | (byte)(r * diffuse) << 16 | (byte)(g * diffuse) << 8 | (byte)(b * diffuse));
        }

        public void ShadeFragmentsWithTexture(IFrameBuffer frameBuffer, List<Vector3> lightSources, IReadOnlyList<IFragment> fragments)
        {
            var bucketSize = fragments.Count / Constants.NumberOfThreads;

            Parallel.For(0, Constants.NumberOfThreads, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, threadId =>
            {
                var startIndex = threadId * bucketSize;

                for (var i = startIndex; i < startIndex + bucketSize; i++)
                {
                    var fragment = fragments[i];
                    var color = ShadeTexturedFragment(lightSources, fragment);
                    frameBuffer.SetPixelColor((int)fragment.ScreenCoordinates.X, (int)fragment.ScreenCoordinates.Y, (float)fragment.Depth, color.A, color.R, color.B, color.G);
                }
            });
        }

        private Color ShadeTexturedFragment(List<Vector3> lightSources, IFragment fragment)
        {
            var opacity = Globals.NormalizedOpacity;
            var diffuse = 0.0;

            Vector3 barycentricCoordinates = fragment.BarycentricCoordinates;
            var barX = barycentricCoordinates.X;
            var barY = barycentricCoordinates.Y;
            var barZ = barycentricCoordinates.Z;

            foreach (var lightSource in lightSources)
            {
                var interpolatedNormal = (fragment.V0.Normal * barX + fragment.V1.Normal * barY + fragment.V2.Normal * barZ).Normalize();
                var worldPosition = fragment.V0.Position * barX + fragment.V1.Position * barY + fragment.V2.Position * barZ;
                var lightDirection = (worldPosition - lightSource).Normalize();

                var lightAngle = Vector3.Dot(interpolatedNormal, lightDirection);
                diffuse += (-lightAngle).Clamp(0, 1);
            }

            diffuse = diffuse.Clamp(0, 1);

            var textureColor = GetFragmentTextureColor((TexturedFragment)fragment);

            var texturedFragmentColor = Color.FromArgb(
                (byte)(opacity * 255),
                (byte)(textureColor.R * diffuse),
                (byte)(textureColor.G * diffuse),
                (byte)(textureColor.B * diffuse));

            return texturedFragmentColor;
        }

        private Color GetFragmentTextureColor(in TexturedFragment fragment)
        {
            var textureCoords = fragment.TextureCoordinates;

            var color = _texture.GetTextureColor(textureCoords.X, textureCoords.Y, Globals.TextureInterpolation);

            return color;
        }
    }
}
