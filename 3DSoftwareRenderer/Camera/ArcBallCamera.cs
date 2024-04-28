using System.Numerics;

namespace SoftwareRenderer3D.Camera
{
    public class ArcBallCamera
    {
        private Vector3 _position;
        private Matrix4x4 _rotation;
        public ArcBallCamera(Vector3 initialPosition) {
            _position = initialPosition;
        }
        
        /// <summary>
        /// Returns the view matrix of the camera looking at the target point.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Matrix4x4 LookAt(Vector3 target)
        {
            var forward = Vector3.Normalize(_position - target);
            var upDir = Vector3.UnitY;
            var left = Vector3.Cross(forward, upDir);
            var up = Vector3.Cross(forward, left);

            var rotationMatrix = new Matrix4x4(
                left.X,    left.Y,    left.Z,    0,
                up.X,      up.Y,      up.Z,      0,
                forward.X, forward.Y, forward.Z, 0,
                0,         0,         0,         1);

            var translationMatrix = new Matrix4x4(
                1, 0, 0, -_position.X,
                0, 1, 0, -_position.Y,
                0, 0, 1, -_position.Z,
                0, 0, 0, 1);

            var viewMatrix = rotationMatrix * translationMatrix;

            return viewMatrix;
        }
        /// <summary>
        /// Returns the view matrix of the camera looking at world origin.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 LookAt()
        {
            var forward = Vector3.Normalize(-_position);
            var upDir = Vector3.UnitY;
            var left = Vector3.Cross(forward, upDir);
            var up = Vector3.Cross(forward, left);

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

            var viewMatrix = rotationMatrix * translationMatrix;

            return viewMatrix;
        }
    }
}
