using Microsoft.Win32;
using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Factories;
using SoftwareRenderer3D.RenderContexts;
using SoftwareRenderer3D.Renderers;
using SoftwareRenderer3D.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
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

        private readonly RenderContext _renderContext;

        private Queue<double> _fps = new Queue<double>();

        private int _lastX;
        private int _lastY;

        private string _openedFileName;

        private bool _visualizationUpToDate = false;

        public MainViewModel(float width, float height)
        {
            _width = width;
            _height = height;

            var camera = new ArcBallCamera(new Vector3(0, 0, 3), Vector3.Zero);

            _renderContext = new RenderContext((int)width, (int)height, 100, camera);

            var texturePath = @"E:\FINKI\000Diplmoska\3DSoftwareRenderer\3DSoftwareRenderer\Models\dae\textures\cowboy.bmp";
            var texture = new Texture(new Bitmap(texturePath), true);
            _renderContext.BindTexture(texture);
        }

        public void LoadModel()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All files|*.stl;*.dae|STL Files (*.stl)|*.stl| Collada files (*.dae)|*.dae",
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var filePath = openFileDialog.FileName;
                var fileName = openFileDialog.SafeFileName;

                OpenedFileName = fileName;

                _mesh = FileReaderFactory.GetFileReader(filePath).ReadFile(filePath);
            }
            UpToDate = false;
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

        public bool UpToDate
        {
            get => _visualizationUpToDate;
            set
            {
                _visualizationUpToDate = value;
                RaisePropertyChanged(nameof(UpToDate));
                if(!value)
                    Render();
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

        public string FPS
        {
            get => $"FPS: {(_fps.Sum() / _fps.Count):#.##}";
            set
            {
                if (_fps.Count == 15)
                    _fps.Dequeue();
                _fps.Enqueue(double.Parse(value));

                RaisePropertyChanged(nameof(FPS));
            }
        }

        public int Opacity
        {
            get => (int)(Globals.Opacity * 10);
            set
            {
                Globals.Opacity = value / 10.0;
                RaisePropertyChanged(nameof(Opacity));
                UpToDate = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            var tempBitmap = new Bitmap(bitmap);
            using (MemoryStream memory = new MemoryStream())
            {
                try
                {
                    tempBitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                }
                catch { }
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
            _renderContext.Rotate(_width, _height, previousMouseCoords, mouseCoords);
            SetMouse(mouseCoords.X, mouseCoords.Y);
            UpToDate = false;
        }

        public void Resize(float width, float height)
        {
            _width = width;
            _height = height;
            _renderContext.Resize(width, height);

            UpToDate = false;
        }

        public void UpdateZoom(bool reduce)
        {
            _renderContext.Zoom(reduce);
            UpToDate = false;
        }

        public void Render()
        {
            var startTime = DateTime.Now;
            _renderContext.FrameBuffer.Update((int)_width, (int)_height);
            var bitmap = (false) 
                ? TransparencyRenderer.Render(_mesh, _renderContext.FrameBuffer, _renderContext.Camera, _renderContext.Texture)
                : SimpleRenderer.Render(_mesh, _renderContext.FrameBuffer, _renderContext.Camera, _renderContext.Texture);

            RenderTarget = BitmapToImageSource(bitmap);

            var renderTime = (DateTime.Now - startTime).TotalMilliseconds;
            FPS = (1.0 / (renderTime / 1000.0)).ToString();
            UpToDate = true;
        }

        public void SetMouse(float x, float y)
        {
            _lastX = (int) x;
            _lastY = (int) y;
        }
    }
}
