using SoftwareRenderer3D.Camera;
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

        private FrameBuffer _frameBuffer;
        private ArcBallCamera _camera;

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

            _frameBuffer = new FrameBuffer(width, height);
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
        public FrameBuffer FrameBuffer => _frameBuffer;

        public Matrix4x4 GetProjectionMatrix()
        {
            return _camera.ProjectionMatrix;
        }

        public Bitmap GetFrame()
        {
            return _frameBuffer.GetFrame();
        }

        public void ColorPixel(int xInt, int yInt, float z, Color color)
        {
            _frameBuffer.ColorPixel(xInt, yInt, z, color);
        }

        public void Rotate(float width, float height, Vector3 previousMouseCoords, Vector3 newMouseCoords)
        {
            _width = width;
            _height = height;

            _camera.Rotate(width, height, _fov, previousMouseCoords, newMouseCoords);
            _frameBuffer.Update(width, height);
        }
        public void Rotate(Vector3 previousMouseCoords, Vector3 mouseCoords)
        {
            _camera.Rotate(_width, _height, previousMouseCoords, mouseCoords);
        }

        public void Resize(float width, float height)
        {
            _width = width;
            _height = height;

            _camera.Zoom(width, height, _fov);
            _frameBuffer.Update(width, height);
        }

        

        public void Zoom(bool zoomOut)
        {
            _fov *= zoomOut ? 1.1f : 0.9f;
            _fov = MathUtils.Clamp(_fov, 1, 160);
            _camera.Zoom(_width, _height, _fov);
            _frameBuffer.Update(_width, _height);
        }
    }
}
