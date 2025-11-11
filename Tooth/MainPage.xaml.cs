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
using System.Text.Json;
using System.Windows.Input;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.System;
using Windows.UI;
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
    /// An empty page that can be used on its own or navigated to within a <see cref="Frame">.
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
            Backend.Instance.Send("get-supported-resolutions");
            Backend.Instance.Send("get-resolution");
            Backend.Instance.Send("get-fps-limiter-value");
            Backend.Instance.Send("get-fps-limiter-enabled");
            Backend.Instance.Send("get-Frame-Sync-Mode");
            Backend.Instance.Send("get-Low-Latency-Mode");
            Backend.Instance.Send("get-Scaling");
            Backend.Instance.Send("get-auto-suspend");
            Backend.Instance.Send("get-go-back-to-sleep");
            Backend.Instance.Send("init");
        }

        private void PanelSwitch(bool isBackendAlive)
        {
            if (isBackendAlive)
            {
                StartingBackgroundserviceTextBlock.Visibility = Visibility.Collapsed;
                LaunchBackendButton.IsTapEnabled = false;
            }
            else
            {
                StartingBackgroundserviceTextBlock.Visibility = Visibility.Visible;
                LaunchBackendButton.IsTapEnabled = true;
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
                    _model.Resolution = int.Parse(args[1]);
                    _model.SetResolutionVar(int.Parse(args[1]));
                    Trace.WriteLine($"[MainPage.xaml.cs] Recieved Resolution id: {_model.Resolution}");

                    ResolutionComboBox.SelectedValue = _model.Resolution;
                    break;
                case "launch-gamebar-widget":
                    Trace.WriteLine($"[MainPage.xaml.cs] Recieved launch-gamebar-widget");
                    launchGameBarWidget();
                    break;
                case "Scaling":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating GPU-Scaling Mode to {args[1]}");
                    if (args[1] == "0")
                    {
                        _model.DeviceScaling = 0; // Display Maintain Aspect Ratio
                    }
                    else if (args[1] == "1")
                    {
                        _model.DeviceScaling = 1; // GPU
                        _model.GpuScalingMode = 0; // Aspect Ratio
                    }
                    else if (args[1] == "2")
                    {
                        _model.DeviceScaling = 1; // GPU
                        _model.GpuScalingMode = 1; // Stretch
                    }
                    else if (args[1] == "3")
                    {
                        _model.DeviceScaling = 1; // GPU
                        _model.GpuScalingMode = 2; // Center
                    }
                    else if (args[1] == "4")
                    {
                        _model.DeviceScaling = 2; // Retro Scaling
                        _model.RetroScalingMode = 0; // Integer

                    }
                    else if (args[1] == "5")
                    {
                        _model.DeviceScaling = 2; // Retro Scaling
                        _model.RetroScalingMode = 1; // Nearest Neighbor
                    }
                    else
                    {
                        Trace.WriteLine($"[MainPage.xaml.cs] Wrong value Scaling to {args[1]}");
                    }
                    break;
                case "supported-resolutions":

                    args = message.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
                    if (args.Length < 2)
                    {
                        Trace.WriteLine("Malformed message: missing JSON payload");
                        break;
                    }
                    try
                    {
                        if (args.Length < 2)
                            return;
                        Trace.WriteLine("Raw payload: " + args[1]);
                        _model.Resolutions = System.Text.Json.JsonSerializer.Deserialize<List<Resolution>>(args[1]);
                        ResolutionComboBox.ItemsSource = _model.Resolutions;

                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine($"Failed to parse resolutions: {ex.Message}");
                    }

                    break;

                case "auto-suspend":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI Auto Suspend Enabled to {args[1]}");
                    _model.AutoSuspendEnabled = Convert.ToBoolean(int.Parse(args[1]));
                    AutoSuspendToggle.IsOn = _model.AutoSuspendEnabled;
                    break;
                case "go-back-to-sleep":
                    Trace.WriteLine($"[MainPage.xaml.cs] Updating UI Go back to sleep Enabled to {args[1]}");
                    _model.GoBackToSleepEnabled = Convert.ToBoolean(int.Parse(args[1]));
                    GoBackToSleepToggle.IsOn = _model.GoBackToSleepEnabled;
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

            if (ResolutionComboBox.SelectedValue is int resolution && resolution != 0)
            {
                Trace.WriteLine($"[MainPage.xaml.cs] Resolution selected is not native, disabling Display scaling option");
                // Disable "Display" option on the slider
                ScalingDeviceSlider.Minimum = 1;

                ScalingDeviceLabel1.Text = "GPU";
                ScalingDeviceLabel2.Text = "";
                ScalingDeviceLabel3.Text = "Retro";
            }
            else
            {
                Trace.WriteLine($"[MainPage.xaml.cs] Resolution selected is native, enabling Display scaling option");
                // Enable full range
                ScalingDeviceSlider.Minimum = 0;

                ScalingDeviceLabel1.Text = "Display";
                ScalingDeviceLabel2.Text = "GPU";
                ScalingDeviceLabel3.Text = "Retro";
            }

            if (sender is ComboBox combo && combo.SelectedItem is ComboBoxItem item)
            {
                // Extract the Tag (0, 1, or 2)
                if (item.Tag is int tagValue)
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

        private void ScalingDeviceSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (ResolutionComboBox.SelectedValue is int resolution && resolution != 0)
            {
                if (ScalingDeviceSlider.Value < 1)
                {
                    ScalingDeviceSlider.Value = 1; // force slider to GPU
                }
            }

                int value = (int)e.NewValue;
            // Decide which property to bind to based on slider position
            Windows.UI.Xaml.Data.Binding newBinding = null;

            switch (value)
            {
                case 0: // Display
                    BindingOperations.SetBinding(ScalingModeSlider, Slider.ValueProperty, new Windows.UI.Xaml.Data.Binding());
                    return;

                case 1: // GPU
                    newBinding = new Windows.UI.Xaml.Data.Binding
                    {
                        Path = new PropertyPath("GpuScalingMode"),
                        Mode = BindingMode.TwoWay
                    };

                    ScalingModeSlider.Minimum = 0;
                    ScalingModeSlider.Maximum = 2; 
                    Label1.Text = "Aspect Ratio";
                    Label2.Text = "Stretch";
                    Label3.Text = "Center";
                    Label2.Visibility = Visibility.Visible; 
                    break;

                case 2: // Retro
                    newBinding = new Windows.UI.Xaml.Data.Binding
                    {
                        Path = new PropertyPath("RetroScalingMode"),
                        Mode = BindingMode.TwoWay
                    };
                    ScalingModeSlider.Minimum = 0;
                    ScalingModeSlider.Maximum = 1; 
                    Label1.Text = "Integer";
                    Label2.Text = "";
                    Label3.Text = "Nearest Neighbor";
                    Label2.Visibility = Visibility.Collapsed;
                    break;
                default:
                    BindingOperations.SetBinding(ScalingModeSlider, Slider.ValueProperty, new Windows.UI.Xaml.Data.Binding());
                    return;
            }
            // Apply new binding dynamically
            BindingOperations.SetBinding(ScalingModeSlider, Slider.ValueProperty, newBinding);
        }

        private void SecondaryGrid_Loaded(object sender, RoutedEventArgs e)
        {
            double value = ScalingDeviceSlider.Value;

            switch (value)
            {
                case 0: // Display
                    break;

                case 1: // GPU
                    ScalingModeSlider.Minimum = 0;
                    ScalingModeSlider.Maximum = 2;
                    Label1.Text = "Aspect Ratio";
                    Label2.Text = "Stretch";
                    Label3.Text = "Center";
                    Label2.Visibility = Visibility.Visible;
                    break;

                case 2: // Retro
                    ScalingModeSlider.Minimum = 0;
                    ScalingModeSlider.Maximum = 1;
                    Label1.Text = "Integer";
                    Label2.Text = "";
                    Label3.Text = "Nearest Neighbor";
                    Label2.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void OnResumeButtonClick(object sender, RoutedEventArgs e)
        {
            _model.ResumeActiveGame();
        }

        private void OnSuspendButtonClick(object sender, RoutedEventArgs e)
        {
            _model.SuspendActiveGame();
        }

        private void AutoSuspedToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // handle Auto Suspend toggle changes

        }

        private void PowerButtonActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle Power Button Presses
        }

        private void EnhancedSleepToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // handle Enhanced Sleep toggle changes

        }

        private void GoBackToSleepToggle_Toggled(object sender, RoutedEventArgs e)
        {
            // handle Go back to sleep changes

        }
    }
}
