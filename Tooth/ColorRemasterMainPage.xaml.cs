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
        private static ColorRemasterModel _modelBase = new ColorRemasterModel();
        private ColorRemasterModelWrapper _model;

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
            _model = _modelBase.GetWrapper(Dispatcher);
            this.DataContext = _model;
            UpdateImage();

            Backend.Instance.MessageReceivedEvent += Backend_OnMessageReceived;
            Backend.Instance.ClosedOrFailedEvent += Backend_OnClosedOrFailed;
            if (Backend.Instance.IsConnected)
                ConnectedInitialize();
            else
                PanelSwitch(false);
        }

        private void UpdateImage()
        {
            CarouselImage.Source = new BitmapImage(new Uri(_images[_currentIndex]));
        }

        public void Dispose()
        {
            Backend.Instance.MessageReceivedEvent -= Backend_OnMessageReceived;
            Backend.Instance.ClosedOrFailedEvent -= Backend_OnClosedOrFailed;
        }

        private void ConnectedInitialize()
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PanelSwitch(true));
            Backend.Instance.Send("get-Hue-Value");
            Backend.Instance.Send("get-Saturation-Value");
            Backend.Instance.Send("get-Brightness-Value");
            Backend.Instance.Send("get-Contrast-Value");
            Backend.Instance.Send("get-Sharpness-Value");
            Backend.Instance.Send("get-Gamma-Value");
        }

        private void PanelSwitch(bool isBackendAlive)
        {
            if (isBackendAlive)
            {
                StartingBackgroundserviceTextBlock.Visibility = Visibility.Collapsed;
                LaunchBackendButton.IsTapEnabled = false;
                LaunchBackendButton.IsTabStop = false;
            }
            else
            {
                StartingBackgroundserviceTextBlock.Visibility = Visibility.Visible;
                LaunchBackendButton.IsTapEnabled = true;
                LaunchBackendButton.IsTabStop = true;
            }
        }

        private void Backend_OnMessageReceived(object sender, string message)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => Backend_OnMessageReceived_Impl(sender, message));
        }

        private void Backend_OnMessageReceived_Impl(object sender, string message)
        {
            var backend = sender as Backend;
            string[] args = message.Split(' ');
            if (args.Length == 0)
                return;
            switch (args[0])
            {
                case "connected":
                    ConnectedInitialize();
                    break;
                case "Saturation-Value":
                    _model.SaturationValue = int.Parse(args[1]);
                    SliderSaturation.Value = _model.SaturationValue;
                    break;
                case "Contrast-Value":
                    _model.ContrastValue = int.Parse(args[1]);
                    SliderContrast.Value = _model.ContrastValue;
                    break;
                case "Brightness-Value":
                    _model.BrightnessValue = int.Parse(args[1]);
                    SliderBrightness.Value = _model.BrightnessValue;
                    break;
                case "Sharpness-Value":
                    _model.SharpnessValue = int.Parse(args[1]);
                    SliderSharpness.Value = _model.SharpnessValue;
                    break;
                case "Gamma-Value":
                    _model.GammaValue = int.Parse(args[1]);
                    SliderGamma.Value = _model.GammaValue;
                    break;
                case "Hue-Value":
                    _model.HueValue = int.Parse(args[1]);
                    SliderHue.Value = _model.HueValue;
                    break;
            }
        }

        private void Backend_OnClosedOrFailed(object _, EventArgs args)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PanelSwitch(false));
        }

        private void LaunchBackendButton_OnClick(object sender, RoutedEventArgs e)
        {
            _ = Backend.LaunchBackend();
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
            Backend.Instance.Send($"set-Brightness-Value {_model.BrightnessValue}");
        }
        private void SliderContrast_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Backend.Instance.Send($"set-Contrast-Value {_model.ContrastValue}");
        }
        private void SliderGamma_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Backend.Instance.Send($"set-Gamma-Value {_model.GammaValue}");
        }
        private void SliderSaturation_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Backend.Instance.Send($"set-Saturation-Value {_model.SaturationValue}");
        }
        private void SliderHue_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Backend.Instance.Send($"set-Hue-Value {_model.HueValue}");
        }
        private void SliderSharpness_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Backend.Instance.Send($"set-Sharpness-Value {_model.SharpnessValue}");
        }

        private void ResetToDefaultsButton_OnClick(object sender, RoutedEventArgs e)
        {
            _model.ContrastValue = 50;
            _model.SaturationValue = 50;
            _model.BrightnessValue = 50;
            _model.HueValue = 0;
            _model.SharpnessValue = 0;
            _model.GammaValue = 1;

            Backend.Instance.Send($"set-Brightness-Value {_model.BrightnessValue}");
            Backend.Instance.Send($"set-Contrast-Value {_model.ContrastValue}");
            Backend.Instance.Send($"set-Gamma-Value {_model.GammaValue}");
            Backend.Instance.Send($"set-Saturation-Value {_model.SaturationValue}");
            Backend.Instance.Send($"set-Hue-Value {_model.HueValue}");
            Backend.Instance.Send($"set-Sharpness-Value {_model.SharpnessValue}");
        }
    }
}
