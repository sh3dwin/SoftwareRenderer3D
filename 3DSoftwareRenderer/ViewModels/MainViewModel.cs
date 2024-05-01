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
        private SimpleRenderer _renderer;

        private int _lastX;
        private int _lastY;

        private string _openedFileName;

        public MainViewModel(float width, float height)
        {
            _width = width;
            _height = height;

            var filepath = @"E:\FINKI\000Diplmoska\3DSoftwareRenderer\3DSoftwareRenderer\Models\bunny.stl";

            _mesh = FileReaderFactory.GetFileReader(filepath).ReadFile(filepath);

            var camera = new ArcBallCamera(new Vector3(0, 0, 15), Vector3.Zero);

            _renderer = new SimpleRenderer(new RenderContext((int)_width, (int)_height, 150, camera));
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

        public void Update(Vector3 mouseCoords)
        {
            var previousMouseCoords = new Vector3(_lastX, _lastY, 0);
            _lastX = (int)mouseCoords.X;
            _lastY = (int)mouseCoords.Y;

            _renderer.Update(_width, _height, previousMouseCoords, mouseCoords);

            RenderTarget = BitmapToImageSource(_renderer.Render(_mesh));
        }

        public void Update(float width, float height)
        {
            _width = width;
            _height = height;

            _renderer.Update(width, height);

            RenderTarget = BitmapToImageSource(_renderer.Render(_mesh));
        }

        public void SetMouse(float x, float y)
        {
            _lastX = (int) x;
            _lastY = (int) y;
        }


    }
}
