using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.FrameBuffers;
using SoftwareRenderer3D.Utils.GeneralUtils;
using System.Drawing;
using System.Numerics;

namespace SoftwareRenderer3D.RenderContexts
{
    public class RenderContext
    {
        private float _width;
        private float _height;

        private float _fov;

        private IFrameBuffer _frameBuffer;
        private ArcBallCamera _camera;
        private Texture _texture = null;

        public RenderContext(int width, int height, float fov) 
        {
            _height = height;
            _width = width;

            _fov = fov;

            _frameBuffer = new FrameBuffer(width, height);
            _camera = new ArcBallCamera(new Vector3(0, 0, 1), Vector3.Zero);
        }

        public RenderContext(int width, int height, float fov, ArcBallCamera camera)
        {
            _height = height;
            _width = width;

            _fov = fov;

            _frameBuffer = new DepthPeelingBuffer(width, height);
            _camera = camera;
        }

        public RenderContext(FrameBuffer frameBuffer, ArcBallCamera camera)
        {
            _frameBuffer = frameBuffer;
            _camera = camera;
        }

        public float Width => _width;
        public float Height => _height;

        public ArcBallCamera Camera => _camera;
        public IFrameBuffer FrameBuffer => _frameBuffer;
        public Texture Texture => _texture;

        public void Rotate(float width, float height, Vector3 previousMouseCoords, Vector3 newMouseCoords)
        {
            _width = width;
            _height = height;

            _camera.Rotate(width, height, _fov, previousMouseCoords, newMouseCoords);
            _frameBuffer.Update((int)width, (int)height);
        }

        public void Resize(float width, float height)
        {
            _width = width;
            _height = height;

            _camera.Zoom(width, height, _fov);
            _frameBuffer.Update((int)width, (int)height);
        }
        public void BindTexture(Texture texture)
        {
            _texture = texture;
        }

        public void UnbindTexture()
        {
            _texture = null;
        }
        public void Zoom(bool zoomOut)
        {
            _fov *= zoomOut ? 1.1f : 0.9f;
            _fov = MathUtils.Clamp(_fov, 1, 160);
            _camera.Zoom(_width, _height, _fov);
            _frameBuffer.Update((int)_width, (int)_height);
        }
    }
}
