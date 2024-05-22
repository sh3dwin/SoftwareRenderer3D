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

            _viewModel = new MainViewModel(800, 200);

            OpacitySlider.Value = _viewModel.Opacity;

            DataContext = _viewModel;

            RenderTarget.MouseMove += Rotate;
            SizeChanged += Resize;
            RenderTarget.MouseWheel += Zoom;
        }

        private void Rotate(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _viewModel.SetMouse((float)e.GetPosition(RenderTarget).X, (float)e.GetPosition(RenderTarget).Y);
                return;
            }

            var mousePos = new Vector3((float)e.GetPosition(RenderTarget).X, (float)e.GetPosition(RenderTarget).Y, 0);

            _viewModel.Rotate(mousePos);
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

            var width = (float)ActualWidth;
            var height = (float)ActualHeight;

            _viewModel.Resize(width, height);
        }
    }
}
