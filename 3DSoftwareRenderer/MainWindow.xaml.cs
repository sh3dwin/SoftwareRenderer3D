using SoftwareRenderer3D.ViewModels;
using System.Windows;

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

            _viewModel = new MainViewModel();

            DataContext = _viewModel;
        }
    }
}
