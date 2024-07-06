using SoftwareRenderer3D.Maths;
using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Security.Principal;
using System.Windows.Input;

namespace SoftwareRenderer3D.Camera
{
    public class ArcBallCamera
    {
        private Vector3 _position;
        private Vector3 _lookAt;

        private Matrix4x4 _viewMatrix;

        private Matrix4x4 _projectionMatrix;

        private float _nearPlane;
        private float _farPlane;

        private float _right;
        private float _left;
        private float _top;
        private float _bottom;

        public ArcBallCamera(Vector3 initialPosition, Vector3 lookAt)
        {

            _nearPlane = Constants.NearFrustumPlaneDistance;
            _farPlane = Constants.FarFrustumPlaneDistance;

            _position = initialPosition;
            _lookAt = lookAt;

            CalculateView();
        }

        public Matrix4x4 ViewMatrix => _viewMatrix;
        public Matrix4x4 ProjectionMatrix => _projectionMatrix;

        /// <summary>
        /// Returns the view matrix of the camera looking at world origin.
        /// </summary>
        /// <returns></returns>

        /// <summary>
        /// Updates the camera position by rotating by delta X and delta Y
        /// http://asliceofrendering.com/camera/2019/11/30/ArcballCamera/
        /// </summary>
        public void Rotate(float width, float height, Vector3 firstPixel, Vector3 secondPixel)
        {
            var ndcSecond = ProjectOnArcBall(width, height, firstPixel);
            var ndcFirst = ProjectOnArcBall(width, height, secondPixel);

            var angle = -(float)Math.Acos(Math.Min(1, Vector3.Dot(ndcFirst.Normalize(), ndcSecond.Normalize())));
            var axis = Vector3.Cross(ndcFirst, ndcSecond).Normalize();

            var rotation = MathUtils.RotateAroundAxis(angle, axis);

            _position = _position.TransformHomogeneus(rotation).ToVector3();
            CalculateView();
        }

        public void RotateUsingQuaternions(float width, float height, Vector3 firstPixel, Vector3 secondPixel)
        {
            var ndcFirst = ProjectOnArcBall(width, height, firstPixel).Normalize();
            var ndcSecond = ProjectOnArcBall(width, height, secondPixel).Normalize();

            var angle = Math.Acos(Vector3.Dot(ndcFirst, ndcSecond)).Clamp(0, Math.PI);
            var axis = Vector3.Cross(ndcFirst, ndcSecond);

            Debug.WriteLine($"First point on inverted trumpet: {ndcFirst}");
            Debug.WriteLine($"Second point on inverted trumpet: {ndcSecond}");

            Debug.WriteLine($"Rotation angle: {angle}");
            Debug.WriteLine($"Camera position: {_position}");

            var newPos = _position.RotateAroundAxis(axis, angle);

            _position = newPos;
            CalculateView();
        }


        public void Rotate(float width, float height, float fov, Vector3 previousMouseCoords, Vector3 newMouseCoords)
        {
            Debug.WriteLine($"Pivot mouse coordinates: ({previousMouseCoords.X}, {previousMouseCoords.Y})");
            Debug.WriteLine($"New mouse coordinates: ({newMouseCoords.X}, {newMouseCoords.Y})");

            RotateUsingQuaternions(width, height, previousMouseCoords, newMouseCoords);
            UpdateProjectionMatrix(width, height, fov);
        }

        public void Zoom(float width, float height, float fov)
        {
            UpdateProjectionMatrix(width, height, fov);
        }

        public Vector3 Position => _position;

        private void CalculateView()
        {
            var forward = -GetForwardVector().Normalize();
            var upDir = (Vector3.Dot(Vector3.UnitY, forward) > 0.95) ? Vector3.UnitZ : Vector3.UnitY;
            var left = Vector3.Cross(forward, upDir).Normalize();
            var up = Vector3.Cross(forward, left).Normalize();

            var rotationMatrix = new Matrix4x4(
                left.X, left.Y, left.Z, 0,
                up.X, up.Y, up.Z, 0,
                forward.X, forward.Y, forward.Z, 0,
                0, 0, 0, 1);

            var translationMatrix = new Matrix4x4(
                1, 0, 0, -_position.X,
                0, 1, 0, -_position.Y,
                0, 0, 1, -_position.Z,
                0, 0, 0, 1);

            _viewMatrix = rotationMatrix * translationMatrix;
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

            var ndcCoordinates = new Vector3(px, py, 0);
            var d = ndcCoordinates.X * ndcCoordinates.X + ndcCoordinates.Y * ndcCoordinates.Y;

            if (2 * d <= arcBallRadius * arcBallRadius)
                ndcCoordinates.Z = (float)Math.Sqrt(arcBallRadius * arcBallRadius - d);
            else
                ndcCoordinates.Z = (float)(arcBallRadius * arcBallRadius / 2.0 / Math.Sqrt(d));

            return ndcCoordinates;
        }

        private Vector3 GetForwardVector()
        {
            return _lookAt - _position;
        }

        private void CalculateProjection()
        {
            _projectionMatrix = new Matrix4x4(
                2.0f * _nearPlane / (_right - _left), 0, (_right + _left) / (_right - _left), 0,
                0, 2.0f * _nearPlane / (_top - _bottom), (_top + _bottom) / (_top - _bottom), 0,
                0, 0, (-(_farPlane + _nearPlane)) / (_farPlane - _nearPlane), (-(20.0f * _farPlane * _nearPlane)) / (_farPlane - _nearPlane),
                0, 0, -1, 0
                );
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
    }
}
