using g3;
using System.Numerics;

namespace SoftwareRenderer3D.Camera
{
    public class ArcBallCamera
    {
        private Vector3f _position;
        private Matrix4x4 _rotation;
        public ArcBallCamera(Vector3f initialPosition) {
            _position = initialPosition;
        }
        
        /// <summary>
        /// Returns the view matrix of the camera looking at the target point.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public Matrix4x4 LookAt(Vector3f target)
        {
            var forward = (_position - target).Normalized;
            var upDir = Vector3f.AxisY;
            var left = forward.Cross(upDir);
            var up = forward.Cross(left);

            var rotationMatrix = new Matrix4x4(
                left.x,    left.y,    left.z,    0,
                up.x,      up.y,      up.z,      0,
                forward.x, forward.y, forward.z, 0,
                0,         0,         0,         1);

            var translationMatrix = new Matrix4x4(
                1, 0, 0, -_position.x,
                0, 1, 0, -_position.y,
                0, 0, 1, -_position.z,
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
            var forward = (-_position).Normalized;
            var upDir = Vector3f.AxisY;
            var left = forward.Cross(upDir);
            var up = forward.Cross(left);

            var rotationMatrix = new Matrix4x4(
                left.x, left.y, left.z, 0,
                up.x, up.y, up.z, 0,
                forward.x, forward.y, forward.z, 0,
                0, 0, 0, 1);

            var translationMatrix = new Matrix4x4(
                1, 0, 0, -_position.x,
                0, 1, 0, -_position.y,
                0, 0, 1, -_position.z,
                0, 0, 0, 1);

            var viewMatrix = rotationMatrix * translationMatrix;

            return viewMatrix;
        }
    }
}
