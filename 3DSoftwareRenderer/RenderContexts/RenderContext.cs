using SoftwareRenderer3D.Utils;
using System;
using System.Numerics;

namespace SoftwareRenderer3D.RenderContexts
{
    public class RenderContext
    {
        private float _right;
        private float _up;

        private float _width;
        private float _height;

        private float _nearPlane;
        private float _farPlane;

        private Matrix4x4 _worldToNDC;

        public RenderContext(int width, int height, float fov) 
        {
            _width = width;
            _height = height;

            _nearPlane = -Constants.NearFrustumPlaneDistance;
            _farPlane = -Constants.FarFrustumPlaneDistance;

            var degToRad = Math.Acos(-1.0f) / 180.0;

            var tangent = (float)Math.Tan(fov / 2.0f * degToRad);

            _right = _nearPlane * tangent;
            _up = _right * (height / width);

            

            CalculateWorldToNdcMatrix();
        }

        private void CalculateWorldToNdcMatrix()
        {
            _worldToNDC = new Matrix4x4(
                -_nearPlane / _right, 0, 0, 0,
                0, -_nearPlane / _up, 0, 0,
                0, 0, -((_farPlane + _nearPlane) / (_farPlane - _nearPlane)), -((2.0f * _farPlane * _nearPlane) / (_farPlane - _nearPlane)),
                0, 0, -1, 0
                );
        }

        public Matrix4x4 GetProjectionMatrix()
        {
            return _worldToNDC;
        }

        public float Height
        {
            get => _width;
            set 
            { 
                _up = value;
                CalculateWorldToNdcMatrix();
            }
        }

        public float Width
        {
            get => _height;
            set
            { 
                _right = value;
                CalculateWorldToNdcMatrix();
            }
        }

    }
}
