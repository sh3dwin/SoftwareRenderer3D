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

namespace SoftwareRenderer3D.FragmentShaders
{
    public static class SubsurfaceScatteringFragmentShader
    {
        const double Epsilon = 0.1;

        private static Texture _texture = null;

        public static void BindTexture(Texture texture)
        {
            _texture = texture;
        }
        public static void UnbindTexture()
        {
            _texture = null;
        }
        public static void ShadeFragments(IFrameBuffer frameBuffer, List<Vector3> lightSources, List<IFragment> fragments, Dictionary<IVertex, double> subsurfaceScatteringAmount)
        {
            Parallel.ForEach(fragments, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, fragment =>
            {
                var color = ShadeFragment(fragment, lightSources, subsurfaceScatteringAmount);

                frameBuffer.SetPixelColor((int)fragment.ScreenCoordinates.X, (int)fragment.ScreenCoordinates.Y, (float)fragment.Depth, color);
            });
        }
        private static Color ShadeFragment(IFragment fragment, List<Vector3> lightSources, Dictionary<IVertex, double> subsurfaceScatteringAmount)
        {
            var diffuse = 0.0;

            foreach (var lightSource in lightSources)
            {
                var interpolatedNormal =
                    (fragment.V0.Normal * fragment.BarycentricCoordinates.X
                    + fragment.V1.Normal * fragment.BarycentricCoordinates.Y
                    + fragment.V2.Normal * fragment.BarycentricCoordinates.Z).Normalize();
                var worldPosition =
                    fragment.V0.WorldPoint * fragment.BarycentricCoordinates.X
                    + fragment.V1.WorldPoint * fragment.BarycentricCoordinates.Y
                    + fragment.V2.WorldPoint * fragment.BarycentricCoordinates.Z;
                var lightDirection = (worldPosition - lightSource).Normalize();

                diffuse += (-Vector3.Dot(interpolatedNormal, lightDirection)).Clamp();
            }

            var subsurfaceAmount = 0.0;
            if (fragment.BarycentricCoordinates.X > 1 + Epsilon || fragment.BarycentricCoordinates.X < -Epsilon
                || fragment.BarycentricCoordinates.Y > 1 + Epsilon || fragment.BarycentricCoordinates.Y < -Epsilon
                || fragment.BarycentricCoordinates.Z > 1 + Epsilon || fragment.BarycentricCoordinates.Z < -Epsilon)
            {
                IVertex closest;
                if (fragment.BarycentricCoordinates.X > fragment.BarycentricCoordinates.Y)
                {
                    if (fragment.BarycentricCoordinates.X > fragment.BarycentricCoordinates.Z)
                        closest = fragment.V0;
                    else
                        closest = fragment.V2;
                }
                else
                {
                    if (fragment.BarycentricCoordinates.Y > fragment.BarycentricCoordinates.Z)
                        closest = fragment.V1;
                    else
                        closest = fragment.V2;
                }
                subsurfaceAmount = 0 + subsurfaceScatteringAmount[closest];
            }

            else
            {
                subsurfaceAmount = 0 + (
                    subsurfaceScatteringAmount[fragment.V0] * fragment.BarycentricCoordinates.X
                    + subsurfaceScatteringAmount[fragment.V1] * fragment.BarycentricCoordinates.Y
                    + subsurfaceScatteringAmount[fragment.V2] * fragment.BarycentricCoordinates.Z);
            }


            diffuse = diffuse.Clamp(0, 0.6);
            subsurfaceAmount = subsurfaceAmount.Clamp(0, 0.3);

            diffuse += subsurfaceAmount; 

            var color = fragment.V0.Color.Mult(fragment.BarycentricCoordinates.X)
                .Add(fragment.V1.Color.Mult(fragment.BarycentricCoordinates.Y)
                .Add(fragment.V2.Color.Mult(fragment.BarycentricCoordinates.Z)));

            if (fragment.V0.GetType().IsAssignableFrom(typeof(TexturedVertex)) && _texture != null)
            {
                color = GetFragmentTextureColor(fragment);
            }

            var opacity = Globals.NormalizedOpacity.Clamp(0, 255);
            var fragmentColor = Color.FromArgb((int)(opacity * 255), (int)(color.R * diffuse), (int)(color.G * diffuse), (int)(color.B * diffuse));

            return fragmentColor;
        }

        private static Color GetFragmentTextureColor(IFragment fragment)
        {
            var texturePosition =
                (fragment.V0 as TexturedVertex).TextureCoordinates * fragment.BarycentricCoordinates.X
                + (fragment.V1 as TexturedVertex).TextureCoordinates * fragment.BarycentricCoordinates.Y
                + (fragment.V2 as TexturedVertex).TextureCoordinates * fragment.BarycentricCoordinates.Z;

            var color = _texture.GetTextureColor(texturePosition.X, texturePosition.Y, Globals.TextureInterpolation);

            return color;
        }
    }
}
