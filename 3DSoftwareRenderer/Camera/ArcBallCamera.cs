using SoftwareRenderer3D.Utils;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System;
using System.Numerics;

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

        public ArcBallCamera(Vector3 initialPosition, Vector3 lookAt) {

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
        private void CalculateView()
        {
            var forward = -Vector3.Normalize(GetForwardVector());
            var upDir = Vector3.UnitY;
            var left = Vector3.Normalize(Vector3.Cross(forward, upDir));
            var up = Vector3.Normalize(Vector3.Cross(forward, left));

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

        private Vector3 GetForwardVector()
        {
            return (_lookAt - _position);
        }

        private void CalculateProjection()
        {
            _projectionMatrix = new Matrix4x4(
                (2.0f * _nearPlane) / (_right - _left), 0, (_right + _left) / (_right - _left), 0,
                0, (2.0f * _nearPlane) / (_top - _bottom), (_top + _bottom) / (_top - _bottom), 0,
                0, 0, (-(_farPlane + _nearPlane)) / (_farPlane - _nearPlane), (-(20.0f * _farPlane * _nearPlane)) / (_farPlane - _nearPlane),
                0, 0, -1, 0
                );
        }

        public void UpdateProjectionMatrix(float width, float height, float fov)
        {

            var degToRad = Math.Acos(-1.0f) / 180.0;

            var tangent = (float)Math.Tan(fov / 2.0f * degToRad);

            _right = _nearPlane * tangent;
            _top = _right * (height / width);

            _left = -_right;
            _bottom = -_top;

            CalculateProjection();
        }

        /// <summary>
        /// Updates the camera position by rotating by delta X and delta Y
        /// http://asliceofrendering.com/camera/2019/11/30/ArcballCamera/
        /// </summary>
        public void Rotate(float width, float height, Vector3 firstPixel, Vector3 secondPixel)
        {
            var ndcSecond = ConvertToNdc(width, height, firstPixel);
            var ndcFirst = ConvertToNdc(width, height, secondPixel);

            var angle = -(float)Math.Acos(Math.Min(1, Vector3.Dot(Vector3.Normalize(ndcFirst), Vector3.Normalize(ndcSecond))));
            var axis = Vector3.Normalize(Vector3.Cross(ndcFirst, ndcSecond));

            var rotation = MathUtils.RotateAroundAxis(angle, axis);

            _position = _position.TransformHomogeneus(rotation).ToVector3();
            CalculateView();
        }

        private Vector3 ConvertToNdc(float width, float height, Vector3 screenCoordinates)
        {
            var ndcCoordinates = new Vector3((screenCoordinates.X - width / 2.0f) / (width / 2.0f), -(screenCoordinates.Y - height / 2.0f) / (height / 2.0f), 0);


            if ((ndcCoordinates.X * ndcCoordinates.X + ndcCoordinates.Y * ndcCoordinates.Y) <= 1)
                ndcCoordinates.Z = (float)Math.Abs(Math.Sqrt(1 - ndcCoordinates.X * ndcCoordinates.X + ndcCoordinates.Y * ndcCoordinates.Y));
            else
                ndcCoordinates.Z = (1.0f / 2.0f) / (float)(Math.Sqrt(ndcCoordinates.X * ndcCoordinates.X + ndcCoordinates.Y * ndcCoordinates.Y));

            return ndcCoordinates;
        }

        public void Update(float width, float height, float fov, Vector3 previousMouseCoords, Vector3 newMouseCoords)
        {
            Rotate(width, height, previousMouseCoords, newMouseCoords);
            UpdateProjectionMatrix(width, height, fov);
        }

        public void Update(float width, float height, float fov)
        {
            UpdateProjectionMatrix(width, height, fov);
        }

        public Vector3 Position => _position;
    }
}
