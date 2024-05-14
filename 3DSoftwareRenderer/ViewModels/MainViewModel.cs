using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Factories;
using SoftwareRenderer3D.RenderContexts;
using SoftwareRenderer3D.Renderers;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;

namespace SoftwareRenderer3D.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private float _width;
        private float _height;

        private BitmapImage _renderTarget;
        private Mesh<IVertex> _mesh;

        private RenderContext _renderContext;

        private int _lastX;
        private int _lastY;

        private string _openedFileName;

        public MainViewModel(float width, float height)
        {
            _width = width;
            _height = height;

            var filepath = @"E:\FINKI\000Diplmoska\3DSoftwareRenderer\3DSoftwareRenderer\Models\bunny.stl";

            _mesh = FileReaderFactory.GetFileReader(filepath).ReadFile(filepath);

            //_mesh = Globals.TestMesh;

            var camera = new ArcBallCamera(new Vector3(0, 0, 100), Vector3.Zero);

            _renderContext = new RenderContext((int)width, (int)height, 100, camera);
        }

        /// <summary>
        /// The image to be rendered on screen
        /// </summary>
        public BitmapImage RenderTarget
        {
            get => _renderTarget;

            set  {
                _renderTarget = value;
                RaisePropertyChanged(nameof(RenderTarget));
            }
        }

        public string OpenedFileName
        {
            get => _openedFileName ?? "No model selected";
            set
            {
                _openedFileName = value;
                RaisePropertyChanged(nameof(OpenedFileName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public void Rotate(Vector3 mouseCoords)
        {
            var previousMouseCoords = new Vector3(_lastX, _lastY, 0);
            _lastX = (int)mouseCoords.X;
            _lastY = (int)mouseCoords.Y;

            _renderContext.Rotate(_width, _height, previousMouseCoords, mouseCoords);

            Render();
        }

        public void Resize(float width, float height)
        {
            _width = width;
            _height = height;

            _renderContext.Resize(width, height);

            Render();
        }

        public void UpdateZoom(bool reduce)
        {
            _renderContext.Zoom(reduce);

            Render();
        }

        public void Render()
        {
            var bitmap = SimpleRenderer.Render(_mesh, _renderContext.FrameBuffer, _renderContext.Camera);
            RenderTarget = BitmapToImageSource(bitmap);
        }

        public void SetMouse(float x, float y)
        {
            _lastX = (int) x;
            _lastY = (int) y;
        }


    }
}
