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
            Backend.Instance.Send("get-EnduranceGaming");
            Backend.Instance.Send("get-resolution");
            Backend.Instance.Send("get-fps-limiter-value");
            Backend.Instance.Send("get-fps-limiter-enabled");
            Backend.Instance.Send("get-Frame-Sync-Mode");
            Backend.Instance.Send("get-Low-Latency-Mode");
            Backend.Instance.Send("init");
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
                case "fps-limiter-enabled":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI GPU FPS Limiter Enabled to {args[1]}");
                    _model.FpsLimitEnabled =  Convert.ToBoolean(int.Parse(args[1]));
                    FpsLimiterToggle.IsOn = _model.FpsLimitEnabled;
                    break;
                case "fps-limiter-value":
                    _model.FpsLimitValue = int.Parse(args[1]);
                    FPSSlider.Value = _model.FpsLimitValue;
                    break;
                case "boost":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI CPU Boost {args[1]}");
                    _model.BoostMode = double.Parse(args[1]);
                    if (_model.BoostMode< 0)
                    {
                        CpuBoostModeSelector.IsEnabled = false;
                        CpuBoostModeSelector.Opacity = 0.5;
                        CpuBoostModeTextBlock.FontSize = 14;
                        CpuBoostModeTextBlock.Text =
                                                "CPU Boost power changes are disabled in Windows\n" +
                                                "Please refer to the Github FAQ to enable it";
                    } else
                    {
                        CpuBoostModeSelector.IsEnabled = true;
                        CpuBoostModeSelector.Opacity = 1.0;
                        if (CpuBoostModeTextBlock.Text != "CPU Boost Mode") { 
                            CpuBoostModeTextBlock.Text = "CPU Boost Mode";
                            CpuBoostModeTextBlock.FontSize = 18;
                        }
                    }
                    CpuBoostModeSelector.SelectedValue = _model.BoostMode;
                    break;
                case "EnduranceGaming":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI GPU Endurance Gaming to {args[1]}");
                    _model.EnduranceGaming = double.Parse(args[1]);
                    EnduranceGamingComboBox.SelectedValue = _model.EnduranceGaming;
                    break;
                case "Low-Latency-Mode":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI GPU Low Latency Value to {args[1]}");
                    _model.LowLatency = double.Parse(args[1]);
                    LowLatencyComboBox.SelectedValue = _model.LowLatency;
                    break;
                case "Frame-Sync-Mode":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI GPU Frame Sync Mode to {args[1]}");
                    _model.FrameSync = double.Parse(args[1]);
                    FrameSyncComboBox.SelectedValue = _model.FrameSync;
                    break;
                case "autostart":
                    _model.SetAutoStartVar(bool.Parse(args[1]));
                    break;
                case "resolution":
                    _model.Resolution = double.Parse(args[1]);
                    _model.SetResolutionVar(double.Parse(args[1]));
                    break;
                case "launch-gamebar-widget":
                    Trace.WriteLine($"[MainPage.xaml.cs] Recieved launch-gamebar-widget");
                    launchGameBarWidget();
                    break;

            }
        }

        private async void launchGameBarWidget()
        {
            var app = (App)Application.Current;
            var widgetControl = app._xboxGameBarWidgetControl;

            if (widgetControl != null)
            {
                Trace.WriteLine($"[MainPage.xaml.cs] widgetControl.ActivateAsync");
                await widgetControl.ActivateAsync("Tooth.XboxGameBarUI");
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
            // handle EnduranceGaming selection changes
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                // Extract the Tag (0, 1, or 2)
                if (item.Tag is double tagValue)
                {
                    if (DataContext is MainPageModelWrapper model)
                    {
                        _model.EnduranceGaming = tagValue;
                        _model.SetEnduranceGamingVar(tagValue);
                    }
                }
            }
        }

        private void LowLatencyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // handle Xe Low Latency selection changes
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                // Extract the Tag (0, 1, or 2)
                if (item.Tag is double tagValue)
                {
                    if (DataContext is MainPageModelWrapper model)
                    {
                        _model.LowLatency = tagValue;
                        _model.SetLowLatencyVar(tagValue);
                    }
                }
            }
        }

        private void FpsLimiterToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // handle FPS Limiter toggle changes
            if (FpsLimiterToggle.IsOn)
            {
                // When enabled, direct focus down to the slider
                FpsLimiterToggle.XYFocusDown = FPSSlider;

                // Focus the slider when enabling
                FPSSlider.Focus(FocusState.Programmatic);
            } else
            {
                // When disabled, clear it so focus jumps past the collapsed panel
                FpsLimiterToggle.XYFocusDown = null;
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
            CpuBoostModeSelector.SelectedValue = _model.BoostMode;
        }

        private void LaunchIGSButton_OnClick(object sender, RoutedEventArgs e)
        {
            Backend.Instance.Send($"IntelGraphicsSofware");
        }

        private void FrameSyncComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // handle Xe Frame Sync selection changes
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                // Extract the Tag (0, 1, or 2)
                if (item.Tag is double tagValue)
                {
                    if (DataContext is MainPageModelWrapper model)
                    {
                        _model.FrameSync = tagValue;
                        _model.SetFrameSyncVar(tagValue);
                    }
                }
            }
        }

        private void ResolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                // Extract the Tag (0, 1, or 2)
                if (item.Tag is double tagValue)
                {
                    if (DataContext is MainPageModelWrapper model)
                    {
                        _model.Resolution = tagValue;
                        _model.SetResolutionVar(tagValue);
                    }
                }
            }
        }

        private void FPSSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            Backend.Instance.Send($"set-Fps-limiter" + ' ' + $"{Convert.ToInt32(_model.FpsLimitEnabled)}" + ' ' + $"{_model.FpsLimitValue}");
        }
    }
}
