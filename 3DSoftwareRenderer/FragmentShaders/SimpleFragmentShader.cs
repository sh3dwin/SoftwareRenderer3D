using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.FrameBuffers;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.FragmentShaders
{
    public static class SimpleFragmentShader
    {
        private static Texture _texture = null;

        public static void BindTexture(Texture texture)
        {
            _texture = texture;
        }

        public static void UnbindTexture()
        {
            _texture = null;
        }

        public static void ShadeFragments(IFrameBuffer frameBuffer, List<Vector3> lightSources, IReadOnlyList<IFragment> fragments)
        {
            var bucketSize = fragments.Count / Constants.NumberOfThreads;

            Parallel.For(0, Constants.NumberOfThreads, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, threadId =>
            {
                var startIndex = threadId * bucketSize;

                for (var i = startIndex; i < startIndex + bucketSize; i++)
                {
                    var fragment = fragments[i];
                    var color = ShadeFragment(fragment, lightSources);
                    frameBuffer.SetPixelColor((int)fragment.ScreenCoordinates.X, (int)fragment.ScreenCoordinates.Y, (float)fragment.Depth, color);
                }
            });
        }

        private static Color ShadeFragment(in IFragment fragment, List<Vector3> lightSources)
        {
            var diffuse = 0.0f;

            var opacity = Globals.NormalizedOpacity;

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

            return Color.FromArgb((byte)(opacity * 255), (byte)(r * diffuse), (byte)(g * diffuse), (byte)(b * diffuse));
        }

        public static void ShadeFragmentsWithTexture(IFrameBuffer frameBuffer, List<Vector3> lightSources, IReadOnlyList<IFragment> fragments)
        {
            var bucketSize = fragments.Count / Constants.NumberOfThreads;

            Parallel.For(0, Constants.NumberOfThreads, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, threadId =>
            {
                var startIndex = threadId * bucketSize;

                for (var i = startIndex; i < startIndex + bucketSize; i++)
                {
                    var fragment = fragments[i];
                    var color = ShadeTexturedFragment(lightSources, fragment);
                    frameBuffer.SetPixelColor((int)fragment.ScreenCoordinates.X, (int)fragment.ScreenCoordinates.Y, (float)fragment.Depth, color);
                }
            });
        }

        private static Color ShadeTexturedFragment(List<Vector3> lightSources, IFragment fragment)
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

        private static Color GetFragmentTextureColor(in TexturedFragment fragment)
        {
            var textureCoords = fragment.TextureCoordinates;

            var color = _texture.GetTextureColor(textureCoords.X, textureCoords.Y, Globals.TextureInterpolation);

            return color;
        }
    }
}
