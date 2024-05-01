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

            _viewModel = new MainViewModel(200, 200);

            DataContext = _viewModel;

            MouseMove += UpdateVisualization;
            SizeChanged += UpdateWindow;
        }

        private void UpdateVisualization(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                _viewModel.SetMouse((float)e.GetPosition(RenderTarget).X, (float)e.GetPosition(RenderTarget).Y);
                return;
            }

            var mousePos = new Vector3((float)e.GetPosition(RenderTarget).X, (float)e.GetPosition(RenderTarget).Y, 0);

            _viewModel.Update(mousePos);
        }

        private void UpdateWindow(object sender, EventArgs e)
        {

            var width = (float)ActualWidth;
            var height = (float)ActualHeight;

            _viewModel.Update(width, height);
        }
    }
}
