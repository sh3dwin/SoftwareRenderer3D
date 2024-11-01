﻿using Microsoft.Win32;
using SoftwareRenderer3D.Camera;
using SoftwareRenderer3D.DataStructures;
using SoftwareRenderer3D.DataStructures.MeshDataStructures;
using SoftwareRenderer3D.DataStructures.VertexDataStructures;
using SoftwareRenderer3D.Enums;
using SoftwareRenderer3D.Factories;
using SoftwareRenderer3D.RenderContexts;
using SoftwareRenderer3D.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SoftwareRenderer3D.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private float _width;
        private float _height;

        private bool _showStats = false;

        private BitmapImage _renderTarget;
        private Mesh<IVertex> _mesh;

        private readonly RenderContext _renderContext;

        private Queue<double> _fps = new Queue<double>();

        private Vector3 _lastMousePosition;

        private string _openedFileName;
        private bool _fileLoaded;

        private bool _visualizationUpToDate = false;

        private bool _simpleRendering = true;
        private bool _transparentRendering = true;
        private bool _subsurfaceScatteringRendering = true;

        private RenderType _renderType;
        public MainViewModel(float width, float height)
        {
            _width = width;
            _height = height;

            _lastMousePosition = new Vector3(_width / 2.0f, height / 2.0f, 0);

            var camera = new ArcBallCamera(new Vector3(0, 0, 3), Vector3.Zero);

            _renderContext = new RenderContext((int)width, (int)height, 100, camera);

            var textureName = "cowboy.bmp";
            var textureDirectory = Path.Combine(Environment.CurrentDirectory, @"Resources\Models\dae\textures");
            var texturePath = Path.Combine(textureDirectory, textureName);
            // texturePath = @"E:\FINKI\000Diplmoska\pictures\checkerboard.png";
            var bitmap = new Bitmap(texturePath);
            var texture = new Texture(bitmap, true);
            _renderContext.BindTexture(texture);

            _renderType = RenderType.None;
            SimpleRendering = false;
            SubsurfaceScatteringRendering = false;
            TransparentRendering = false;

            IsFileLoaded = false;
        }

        /// <summary>
        /// The image to be rendered on screen
        /// </summary>
        public BitmapImage RenderTarget
        {
            get => _renderTarget;

            set
            {
                _renderTarget = value;
                RaisePropertyChanged(nameof(RenderTarget));
            }
        }

        /// <summary>
        /// Stores the information whether the visualization is UpToDate.
        /// If set to false, will render a new frame.
        /// </summary>
        public bool UpToDate
        {
            get => _visualizationUpToDate;
            set
            {
                _visualizationUpToDate = value;
                RaisePropertyChanged(nameof(UpToDate));
                if (!value)
                    Render();
            }
        }

        /// <summary>
        /// The name of the 3d model file that is open.
        /// </summary>
        public string OpenedFileName
        {
            get
            {
                if (IsFileLoaded)
                    return _openedFileName;
                else
                    return "No model selected";
            }
            set
            {
                _openedFileName = value;
                RaisePropertyChanged(nameof(OpenedFileName));
            }
        }

        /// <summary>
        /// Is there a loaded file containing a 3d model.
        /// </summary>
        public bool IsFileLoaded
        {
            get => _fileLoaded;
            set
            {
                _fileLoaded = value;
                RaisePropertyChanged(nameof(IsFileLoaded));
                RaisePropertyChanged(nameof(OpenedFileName));
                RaisePropertyChanged(nameof(TriangleCount));
                RaisePropertyChanged(nameof(VertexCount));
            }
        }

        /// <summary>
        /// Controls whether various statistics are visible.
        /// </summary>
        public bool ShowStats
        {
            get => _showStats;
            set
            {
                _showStats = value;
                RaisePropertyChanged(nameof(ShowStats));
            }
        }

        /// <summary>
        /// Frames per second.
        /// </summary>
        public string FPS
        {
            get
            {
                return $"FPS: {(_fps.Sum() / _fps.Count):#.##}";
            }
            set
            {
                if (_fps.Count == 15)
                    _fps.Dequeue();
                _fps.Enqueue(double.Parse(value));

                RaisePropertyChanged(nameof(FPS));
            }
        }

        /// <summary>
        /// Triangle count in the mesh.
        /// </summary>
        public string TriangleCount
        {
            get
            {
                if (IsFileLoaded)
                    return $"Number of triangles: {(_mesh != null ? _mesh.FacetCount : 0)}";
                else
                    return string.Empty;
            }
            set
            {
                RaisePropertyChanged(nameof(TriangleCount));
            }
        }

        /// <summary>
        /// Vertex count in the mesh.
        /// </summary>
        public string VertexCount
        {
            get
            {
                if (IsFileLoaded)
                    return $"Number of vertices: {(_mesh != null ? _mesh.VertexCount : 0)}";
                else
                    return string.Empty;
            }
            set
            {
                RaisePropertyChanged(nameof(VertexCount));
            }
        }

        /// <summary>
        /// Opacity of the model.
        /// Used to control the opacity when rendering transparency.
        /// </summary>
        public int Opacity
        {
            get => (int)(Globals.NormalizedOpacity * 10);
            set
            {
                Globals.NormalizedOpacity = value / 10.0;
                RaisePropertyChanged(nameof(Opacity));
                UpToDate = false;
            }
        }

        /// <summary>
        /// Whether the application is in simple rendering mode.
        /// </summary>
        public bool SimpleRendering
        {
            get => _simpleRendering;

            set
            {
                _simpleRendering = value;
                if (value)
                {
                    _renderType = RenderType.Simple;
                    Opacity = Constants.MaximumOpacity;
                    SubsurfaceScatteringRendering = false;
                    TransparentRendering = false;
                }
                RaisePropertyChanged(nameof(SimpleRendering));
                UpToDate = false;
            }
        }

        /// <summary>
        /// Whether the application is in subsurface scattering rendering mode.
        /// </summary>
        public bool SubsurfaceScatteringRendering
        {
            get => _subsurfaceScatteringRendering;

            set
            {
                _subsurfaceScatteringRendering = value;

                if (value)
                {
                    _renderType = RenderType.SubsurfaceScattering;
                    Opacity = Constants.MaximumOpacity;
                    SimpleRendering = false;
                    TransparentRendering = false;
                }
                RaisePropertyChanged(nameof(SubsurfaceScatteringRendering));
                UpToDate = false;
            }
        }

        /// <summary>
        /// Whether the application is in transparent rendering mode.
        /// </summary>
        public bool TransparentRendering
        {
            get => _transparentRendering;

            set
            {
                _transparentRendering = value;

                if (value)
                {
                    _renderType = RenderType.Transparent;
                    SubsurfaceScatteringRendering = false;
                    SimpleRendering = false;
                }
                RaisePropertyChanged(nameof(TransparentRendering));
                UpToDate = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

                try
                {
                    _mesh = FileReaderFactory.GetFileReader(filePath).ReadFile(filePath);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                }

                if (_mesh != null)
                {
                    var fileName = openFileDialog.SafeFileName;

                    OpenedFileName = fileName;
                    _mesh.EnsureMeshQuality();
                    ResetView();
                    IsFileLoaded = true;
                }
                else
                {
                    MessageBox.Show($"Failed to load {openFileDialog.SafeFileName}!");
                    IsFileLoaded = false;
                }
            }
            SimpleRendering = true;
            UpToDate = false;
        }

        public void Rotate(Vector3 mouseCoords)
        {
            _renderContext.Rotate(_width, _height, _lastMousePosition, mouseCoords);
            SetMouse(mouseCoords);
            UpToDate = false;
        }

        internal void Pan(Vector3 currentMousePosition)
        {
            _renderContext.Pan(currentMousePosition, _lastMousePosition);
            SetMouse(currentMousePosition);
            UpToDate = false;
        }

        public void UpdateZoom(bool reduce)
        {
            _renderContext.Zoom(reduce);
            UpToDate = false;
        }

        public void Resize(float width, float height)
        {
            _width = width;
            _height = height;
            _renderContext.Resize(width, height);

            UpToDate = false;
        }

        public void SetMouse(Vector3 newPosition)
        {
            _lastMousePosition = newPosition;
        }

        private void Render()
        {
            var startTime = DateTime.Now;
            _renderContext.FrameBuffer.Update((int)_width, (int)_height);


            Bitmap bitmap = RenderPipelineFactory.GetRenderPipeline(_renderType).Render(_mesh, _renderContext.FrameBuffer, _renderContext.Camera, _renderContext.Texture);

            RenderTarget = BitmapToImageSource(bitmap);

            var renderTime = (DateTime.Now - startTime).TotalMilliseconds;
            FPS = (1.0 / (renderTime / 1000.0)).ToString();
            UpToDate = true;
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

        internal void UnloadModel()
        {
            _mesh = null;
            IsFileLoaded = false;
            UpToDate = false;
        }

        internal void ResetView()
        {
            _renderContext.Camera.Reset();
            UpToDate = false;
        }
    }
}
