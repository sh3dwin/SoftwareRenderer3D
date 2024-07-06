using SoftwareRenderer3D.ViewModels;
using System;
using System.Numerics;
using System.Security.Principal;
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

            OpacitySlider.Value = _viewModel.Opacity;

            DataContext = _viewModel;

            //RenderTarget.MouseDown += SetRotationStart;
            RenderTarget.MouseMove += Rotate;
            SizeChanged += Resize;
            RenderTarget.MouseWheel += Zoom;
        }

        private void SetRotationStart(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }
            var pos = e.GetPosition(RenderTarget);
            var mousePos = new Vector3((float)pos.X, (float)pos.Y, 0);

            _viewModel.SetMouse(mousePos.X, mousePos.Y);
        }

        private void Rotate(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(RenderTarget);
            var mousePos = new Vector3((float)pos.X, (float)pos.Y, 0);
            
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _viewModel.SetMouse(mousePos.X, mousePos.Y);
                return;
            }
            
            _viewModel.Rotate(mousePos);
            _viewModel.SetMouse(mousePos.X, mousePos.Y);
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
    }
}
