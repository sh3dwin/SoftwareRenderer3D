using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.Utils.GeneralUtils;
using SoftwareRenderer3D.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SoftwareRenderer3D.FrameBuffers;
using System.Threading.Tasks;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.DataStructures;
using System.Linq;

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
            var useTexture = fragments.Any(f => f is TexturedVertex);

            Parallel.For(0, Constants.NumberOfThreads, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, threadId =>
            {
                var startIndex = threadId * bucketSize;

                for (var i = startIndex; i < startIndex + bucketSize; i++)
                {
                    var fragment = fragments[i];
                    var color = ShadeFragment(fragment, lightSources, useTexture);
                    frameBuffer.SetPixelColor((int)fragment.ScreenCoordinates.X, (int)fragment.ScreenCoordinates.Y, (float)fragment.Depth, color);
                }
            });
        }

        private static Color ShadeFragment(in IFragment fragment, List<Vector3> lightSources, bool useTexture)
        {
            var diffuse = 0.0;

            foreach (var lightSource in lightSources)
            {
                var interpolatedNormal =
                    (fragment.V0.Normal * fragment.BarycentricCoordinates.X
                    + fragment.V1.Normal * fragment.BarycentricCoordinates.Y
                    + fragment.V2.Normal * fragment.BarycentricCoordinates.Z).Normalize();
                var worldPosition =
                    fragment.V0.Position * fragment.BarycentricCoordinates.X
                    + fragment.V1.Position * fragment.BarycentricCoordinates.Y
                    + fragment.V2.Position * fragment.BarycentricCoordinates.Z;
                var lightDirection = (worldPosition - lightSource).Normalize();

                var lightAngle = Vector3.Dot(interpolatedNormal, lightDirection);
                diffuse += (-lightAngle).Clamp(0, 1);
            }

            diffuse = diffuse.Clamp(0, 1);

            var v0FragmentColor = fragment.V0.Color;
            var v1FragmentColor = fragment.V1.Color;
            var v2FragmentColor = fragment.V2.Color;

            var R = (byte)MathUtils.Clamp(v0FragmentColor.R * fragment.BarycentricCoordinates.X +
                v1FragmentColor.R * fragment.BarycentricCoordinates.Y +
                v2FragmentColor.R * fragment.BarycentricCoordinates.Z, 0, 255);
            var G = (byte)MathUtils.Clamp(v0FragmentColor.G * fragment.BarycentricCoordinates.X +
                v1FragmentColor.G * fragment.BarycentricCoordinates.Y +
                v2FragmentColor.G * fragment.BarycentricCoordinates.Z, 0, 255);
            var B = (byte)MathUtils.Clamp(v0FragmentColor.B * fragment.BarycentricCoordinates.X +
                v1FragmentColor.B * fragment.BarycentricCoordinates.Y +
                v2FragmentColor.B * fragment.BarycentricCoordinates.Z, 0, 255);
            var color = Color.FromArgb(v0FragmentColor.A, R, G, B);

            if (useTexture && _texture != null)
                color = GetFragmentTextureColor(fragment);

            var opacity = Globals.NormalizedOpacity.Clamp(0, 255);
            var fragmentColor = Color.FromArgb((int)(opacity * 255), (int)(color.R * diffuse), (int)(color.G * diffuse), (int)(color.B * diffuse));

            return fragmentColor;
        }

        private static Color GetFragmentTextureColor(in IFragment fragment)
        {
            var texturePosition =
                ((TexturedVertex)fragment.V0).TextureCoordinates * fragment.BarycentricCoordinates.X
                + ((TexturedVertex)fragment.V1).TextureCoordinates * fragment.BarycentricCoordinates.Y
                + ((TexturedVertex)fragment.V2).TextureCoordinates * fragment.BarycentricCoordinates.Z;

            var color = _texture.GetTextureColor(texturePosition.X, texturePosition.Y, Globals.TextureInterpolation);

            return color;
        }
    }
}
