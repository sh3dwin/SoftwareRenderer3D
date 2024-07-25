using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.Utils.GeneralUtils;
using SoftwareRenderer3D.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.FrameBuffers;

namespace SoftwareRenderer3D.FragmentShaders
{
    public static class TextureShader
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
        public static void ShadeFragments(List<Vector3> lightSources, IFrameBuffer frameBuffer, List<SimpleFragment> fragments)
        {
            if (_texture == null)
            {
                throw new Exception("FragmentShader: No texture bound!");
            }

            Parallel.ForEach(fragments, new ParallelOptions() { MaxDegreeOfParallelism = Constants.NumberOfThreads }, fragment =>
            {
                var color = ShadeFragment(fragment, lightSources);

                frameBuffer.SetPixelColor((int)fragment.ScreenCoordinates.X, (int)fragment.ScreenCoordinates.Y, (float)fragment.Depth, color);
            });
        }

        private static Color ShadeFragment(SimpleFragment fragment, List<Vector3> lightSources)
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

            var texturePosition = 
                (fragment.V0 as TexturedVertex).TextureCoordinates * fragment.BarycentricCoordinates.X
                + (fragment.V1 as TexturedVertex).TextureCoordinates * fragment.BarycentricCoordinates.Y
                + (fragment.V2 as TexturedVertex).TextureCoordinates * fragment.BarycentricCoordinates.Z;

            diffuse = diffuse.Clamp(0, 1);

            var color = _texture.GetTextureColor(texturePosition.X, texturePosition.Y, Globals.TextureInterpolation);

            var opacity = Globals.NormalizedOpacity.Clamp(0, 255);
            var fragmentColor = Color.FromArgb((int)(opacity * 255), (int)(color.R * diffuse), (int)(color.G * diffuse), (int)(color.B * diffuse));

            return fragmentColor;
        }
    }
}
