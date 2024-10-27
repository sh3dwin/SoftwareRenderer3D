using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Numerics;

namespace SoftwareRenderer3D.Camera
{
    /// <summary>
    /// Class defining an arcball camera with rotation and zoom using quaternions.
    /// </summary>
    /// Implemented with help from:
    /// http://asliceofrendering.com/camera/2019/11/30/ArcballCamera/
    /// https://raw.org/code/trackball-rotation-using-quaternions/
    /// https://oguz81.github.io/ArcballCamera/
    /// http://courses.cms.caltech.edu/cs171/assignments/hw3/hw3-notes/notes-hw3.html
    /// https://www.khronos.org/opengl/wiki/Object_Mouse_Trackball
    public class ArcBallCamera
    {
        private const double PanScaling = 10;

        private Vector3 _position;
        private Vector3 _lookAt;

        private Vector3 _initialPosition;
        private Vector3 _initialLookAt;

        private Matrix4x4 _viewMatrix;

        private Matrix4x4 _projectionMatrix;

        private float _nearPlane;
        private float _farPlane;


        /// <summary>
        /// Defining the frustum.
        /// </summary>
        private float _right;
        private float _left;
        private float _top;
        private float _bottom;

        private Maths.Quaternion _rotation = Maths.Quaternion.Identity;
        private Maths.Quaternion _prevRotation = Maths.Quaternion.Identity;

        public ArcBallCamera(Vector3 initialPosition, Vector3 lookAt)
        {

            _nearPlane = Constants.NearFrustumPlaneDistance;
            _farPlane = Constants.FarFrustumPlaneDistance;

            _initialPosition = initialPosition;
            _position = _initialPosition;

            _initialLookAt = lookAt;
            _lookAt = _initialLookAt;

            CalculateView();
        }

        /// <summary>
        /// Returns the view matrix of the camera looking at world origin.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;
        public Matrix4x4 RotationMatrix => _rotation.RotationMatrixAlternative();
        public Maths.Quaternion Rotation => _rotation;

        public void Rotate(float width, float height, Vector3 firstPixel, Vector3 secondPixel)
        {
            var ndcFirst = ProjectOnArcBall(width, height, firstPixel).Normalize();
            var ndcSecond = ProjectOnArcBall(width, height, secondPixel).Normalize();

            _rotation = Maths.Quaternion.FromBetweenVectors(ndcFirst, ndcSecond);
            
            _rotation *= _prevRotation;
            _prevRotation = _rotation;

            CalculateView();
        }

        public void Rotate(float width, float height, float fov, Vector3 previousMouseCoords, Vector3 newMouseCoords)
        {
            Rotate(width, height, previousMouseCoords, newMouseCoords);
        }

        internal void Pan(float width, float height, float fov, Vector3 currentMousePosition, Vector3 lastMousePosition)
        {
            var ndcCurrent = currentMousePosition.ToNDC(width, height);
            var ndcLast = lastMousePosition.ToNDC(width, height);

            PanInternal(ndcCurrent, ndcLast);
            CalculateView();
        }

        private void PanInternal(Vector3 currentMousePosition, Vector3 lastMousePosition)
        {
            var pan = currentMousePosition - lastMousePosition;

            _lookAt += Vector3.Multiply(pan, (float)PanScaling);
        }

        public void Zoom(float width, float height, float fov)
        {
            UpdateProjectionMatrix(width, height, fov);
        }

        public Vector3 EyePosition => Vector4.Transform(_position, _rotation.RotationMatrixAlternative()).ToVector3();

        private void CalculateView()
        {
            var rotationMatrix = _rotation.RotationMatrixAlternative();
            var cameraMatrix = new Matrix4x4(
                rotationMatrix.M11, rotationMatrix.M12, rotationMatrix.M13, -_position.X,
                rotationMatrix.M21, rotationMatrix.M22, rotationMatrix.M23, -_position.Y,
                rotationMatrix.M31, rotationMatrix.M32, rotationMatrix.M33, -_position.Z,
                0, 0, 0, 1);

            var lookAtTranslationMatrix = new Matrix4x4(
                1, 0, 0, _lookAt.X,
                0, 1, 0, _lookAt.Y,
                0, 0, 1, _lookAt.Z,
                0, 0, 0, 1);

            var viewMatrix = Matrix4x4.Multiply(lookAtTranslationMatrix, cameraMatrix);

            _viewMatrix = viewMatrix;
        }

        /// <summary>
        /// Reference:
        /// https://raw.org/code/trackball-rotation-using-quaternions/
        /// </summary>
        private Vector3 ProjectOnArcBall(float width, float height, Vector3 screenCoordinates)
        {
            var scale = width > height ? height / 2.0f : width / 2.0f;
            var arcBallRadius = 1.0f;

            var px = (2 * screenCoordinates.X - width + 1) / scale;
            var py = (2 * screenCoordinates.Y - height + 1) / scale;

            var ndcCoordinates = new Vector3(px, -py, 0);
            var d = ndcCoordinates.X * ndcCoordinates.X + ndcCoordinates.Y * ndcCoordinates.Y;

            if (2 * d <= arcBallRadius * arcBallRadius)
                ndcCoordinates.Z = (float)Math.Sqrt(arcBallRadius * arcBallRadius - d);
            else
                ndcCoordinates.Z = (float)(arcBallRadius * arcBallRadius / 2.0 / Math.Sqrt(d));

            return ndcCoordinates;
        }

        private void UpdateProjectionMatrix(float width, float height, float fov)
        {
            var degToRad = Math.Acos(-1.0f) / 180.0;

            var tangent = (float)Math.Tan(fov / 2.0f * degToRad);

            _right = _nearPlane * tangent;
            _top = _right * (height / width);

            _left = -_right;
            _bottom = -_top;

            CalculateProjection();
        }

        private void CalculateProjection()
        {
            _projectionMatrix = new Matrix4x4(
                2.0f * _nearPlane / (_right - _left), 0, (_right + _left) / (_right - _left), 0,
                0, 2.0f * _nearPlane / (_top - _bottom), (_top + _bottom) / (_top - _bottom), 0,
                0, 0, (-(_farPlane + _nearPlane)) / (_farPlane - _nearPlane), (-(20.0f * _farPlane * _nearPlane)) / (_farPlane - _nearPlane),
                0, 0, -1, 0);
        }

        internal void Reset()
        {
            _position = _initialPosition;
            _lookAt = _initialLookAt;

            _rotation = Maths.Quaternion.Identity;
            _prevRotation = _rotation;

            CalculateView();
        }
    }
}
