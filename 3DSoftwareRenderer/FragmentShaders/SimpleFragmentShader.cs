using SoftwareRenderer3D.DataStructures.Fragment;
using SoftwareRenderer3D.Utils.GeneralUtils;
using SoftwareRenderer3D.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SoftwareRenderer3D.FrameBuffers;
using System.Threading.Tasks;

namespace SoftwareRenderer3D.FragmentShaders
{
    public static class SimpleFragmentShader
    {
        public static void ShadeFragments(IFrameBuffer frameBuffer, List<Vector3> lightSources, List<SimpleFragment> fragments)
        {
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

            diffuse = diffuse.Clamp(0, 1);

            var color = fragment.V0.Color.Mult(fragment.BarycentricCoordinates.X)
                .Add(fragment.V1.Color.Mult(fragment.BarycentricCoordinates.Y)
                .Add(fragment.V2.Color.Mult(fragment.BarycentricCoordinates.Z)));

            var opacity = Globals.NormalizedOpacity.Clamp(0, 255);
            var fragmentColor = Color.FromArgb((int)(opacity * 255), (int)(color.R * diffuse), (int)(color.G * diffuse), (int)(color.B * diffuse));

            return fragmentColor;
        }
    }
}
