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

namespace Tooth
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : IDisposable
    {
        private static MainPageModel _modelBase = new MainPageModel();
        private MainPageModelWrapper _model;

        public MainPage()
        {
            InitializeComponent();
            _model = _modelBase.GetWrapper(Dispatcher);
            this.DataContext = _model;

            Backend.Instance.MessageReceivedEvent += Backend_OnMessageReceived;
            Backend.Instance.ClosedOrFailedEvent += Backend_OnClosedOrFailed;
            if (Backend.Instance.IsConnected)
                ConnectedInitialize();
            else
                PanelSwitch(false);
        }

        public void Dispose()
        {
            Backend.Instance.MessageReceivedEvent -= Backend_OnMessageReceived;
            Backend.Instance.ClosedOrFailedEvent -= Backend_OnClosedOrFailed;
        }

        private void ConnectedInitialize()
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => PanelSwitch(true));
            Backend.Instance.Send("get-fps-limit");
            Backend.Instance.Send("get-boost");
        }

        private void PanelSwitch(bool isBackendAlive)
        {
            if (isBackendAlive)
            {
                MainPanel.Visibility = Visibility.Visible;
                BackendPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                MainPanel.Visibility = Visibility.Collapsed;
                BackendPanel.Visibility = Visibility.Visible;
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
                case "fps-limit":
                    _model.FpsMax = double.Parse(args[1]);
                    _model.FpsMin = double.Parse(args[2]);
                    break;
                case "boost":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI CPU Boost {args[1]}");
                    _model.BoostMode = double.Parse(args[1]);
                    CpuBoostModeSelector.SelectedValue = _model.BoostMode;
                    break;
                case "fps":
                    _model.SetFpsVar(double.Parse(args[1]));
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

        private void EnduranceGamingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: handle EnduranceGaming selection changes

        }

        private void LowLatencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: handle Xe Low Latency selection changes
        }

        private void FpsLimiterToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // TODO: handle FPS Limiter toggle changes
            if (FpsLimiterToggle.IsOn)
            {
                // Focus the slider when enabling
                FPSSlider.Focus(FocusState.Programmatic);
            }


        }

        private void CpuBoostModeSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                // Extract the Tag (0, 1, or 2)
                if (item.Tag is double tagValue)
                {
                    // Update your model
                    if (DataContext is MainPageModelWrapper model)
                    {
                        _model.BoostMode = tagValue;
                        _model.SetBoostVar(tagValue);
                    }
                }
            }
        }

        private void CpuBoostModeSelector_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus the slider when enabling
            FPSSlider.Focus(FocusState.Programmatic);

            CpuBoostModeSelector.SelectedValue = _model.BoostMode;
        }

        private void LaunchIGSButton_OnClick(object sender, RoutedEventArgs e)
        {
            Backend.Instance.Send($"IntelGraphicsSofware");
        }
    }
}
