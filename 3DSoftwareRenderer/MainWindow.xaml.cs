using SoftwareRenderer3D.ViewModels;
using System;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace SoftwareRenderer3D
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainViewModel(800, 800);

            //OpacitySlider.Value = _viewModel.Opacity;

            DataContext = _viewModel;

            RenderTarget.MouseMove += ProcessMouseMovement;
            SizeChanged += Resize;
            RenderTarget.MouseWheel += Zoom;
            // RenderTarget.MouseMove += Pan;
        }

        private void ProcessMouseMovement(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(RenderTarget);
            var mousePos = new Vector3((float)pos.X, (float)pos.Y, 0);

            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _viewModel.Pan(mousePos);
            }
            if (e.RightButton == MouseButtonState.Pressed)
            {
                _viewModel.Rotate(mousePos);
            }

            _viewModel.SetMouse(mousePos);
        }

        private void Zoom(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                _viewModel.UpdateZoom(true);
            }
            if ( e.Delta < 0)
            {
                _viewModel.UpdateZoom(false);
            }
        }

        private void Resize(object sender, EventArgs e)
        {

            var width = (float)RenderTarget.ActualWidth;
            var height = (float)RenderTarget.ActualHeight;

            if(width == 0  || height == 0 || true)
            {
                width = (float)ActualWidth;
                height = (float)ActualHeight;
            }    

            _viewModel.Resize(width, height);
        }

        private void OpenFile_Click(object sender, EventArgs e)
        {
            _viewModel.LoadModel();
        }

        private void ClearFile_Click(object sender, EventArgs e)
        {
            _viewModel.UnloadModel();
        }

        private void ShowStatistics_Click(object sender, EventArgs e)
        {
            _viewModel.ShowStats = !_viewModel.ShowStats;
        }

        private void ResetView_Click(object sender, EventArgs e)
        {
            _viewModel.ResetView();
        }
    }
}
