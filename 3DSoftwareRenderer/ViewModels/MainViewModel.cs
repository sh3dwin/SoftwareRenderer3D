using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SoftwareRenderer3D.ViewModels
{
    public class MainViewModel
    {
        private BitmapImage _renderTarget;

        public MainViewModel() { 

        }

        /// <summary>
        /// The image to be rendered on screen
        /// </summary>
        public BitmapImage RenderTarget => _renderTarget;


    }
}
