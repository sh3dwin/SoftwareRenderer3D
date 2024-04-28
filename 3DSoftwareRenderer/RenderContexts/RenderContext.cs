using SoftwareRenderer3D.Utils;
using System.Numerics;

namespace SoftwareRenderer3D.RenderContexts
{
    public class RenderContext
    {
        private float _windowWidth;
        private float _windowHeight;

        private float _nearPlane;
        private float _farPlane;

        private Matrix4x4 _worldToNDC;

        public RenderContext(int width, int height) {
            _windowWidth = width;
            _windowHeight = height;

            _nearPlane = Constants.NearFrustumPlaneDistance;
            _farPlane = Constants.FarFrustumPlaneDistance;

            CalculateWorldToNdcMatrix();
        }

        private void CalculateWorldToNdcMatrix()
        {
            _worldToNDC = new Matrix4x4(
                _nearPlane / (_windowWidth / 2.0f), 0, 0, 0,
                0, _nearPlane / (_windowHeight / 2.0f), 0, 0,
                0, 0, -(_farPlane + _nearPlane) / (_farPlane - _nearPlane), -(2.0f * _farPlane * _nearPlane) / (_farPlane - _nearPlane),
                0, 0, -1, 0
                );
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return _worldToNDC;
        }

        public float Height
        {
            get => _windowHeight;
            set 
            { 
                _windowHeight = value;
                CalculateWorldToNdcMatrix();
            }
        }

        public float Width
        {
            get => _windowWidth;
            set
            { 
                _windowWidth = value;
                CalculateWorldToNdcMatrix();
            }
        }

    }
}
