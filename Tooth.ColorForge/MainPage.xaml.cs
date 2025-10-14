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
    }
}
