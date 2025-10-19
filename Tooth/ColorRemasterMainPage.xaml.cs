using Microsoft.Gaming.XboxGameBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceModel.Channels;
using System.Windows.Input;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;

namespace Tooth
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
    /// </summary>
    public sealed partial class ColorRemasterMainPage : IDisposable
    {
        private string[] _images = new string[]
        {
            "ms-appx:///Assets/stray.png",
            "ms-appx:///Assets/doom.png",
            "ms-appx:///Assets/lastofus.png",
            "ms-appx:///Assets/silksong.png",
            "ms-appx:///Assets/ghostrunner.png",
            "ms-appx:///Assets/hades.png",
            "ms-appx:///Assets/celeste.png"
        };

        private int _currentIndex = 0;


        public ColorRemasterMainPage()
        {
            InitializeComponent();
            UpdateImage();
        }

        private void UpdateImage()
        {
            CarouselImage.Source = new BitmapImage(new Uri(_images[_currentIndex]));
        }

        public void Dispose()
        {
        }

        private void LeftButton_Click(object sender, RoutedEventArgs e)
        {
            _currentIndex--;
            if (_currentIndex < 0) _currentIndex = _images.Length - 1;
            UpdateImage();
        }

        private void RightButton_Click(object sender, RoutedEventArgs e)
        {
            _currentIndex++;
            if (_currentIndex >= _images.Length) _currentIndex = 0;
            UpdateImage();
        }

        private void CarouselImage_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            // Optional: tap to advance to next image
            // We could do better here by showing reference on Press, and current on release
            RightButton_Click(sender, e);
        }

        private void SliderBrightness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
        }
        private void SliderContrast_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
        }
        private void SliderGamma_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
        }
        private void SliderVibrancy_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
        }
        private void SliderHue_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
        }
        private void SliderSharpness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
        }

        private void ResetToDefaultsButton_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}
