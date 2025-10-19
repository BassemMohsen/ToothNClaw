using Microsoft.Gaming.XboxGameBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Profile;
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
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private XboxGameBarWidget _xboxGameBarWidget;
        public XboxGameBarWidgetControl _xboxGameBarWidgetControl { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }


        protected override void OnActivated(IActivatedEventArgs args)
        {
            Trace.WriteLine("[App.xaml.cs] OnActivated");

            if (args.Kind != ActivationKind.Protocol)
                return;

            var protocolArgs = args as IProtocolActivatedEventArgs;
            var uri = protocolArgs?.Uri;

            Trace.WriteLine($"Widget OnActivated uri: {uri}");
            if (uri == null || !uri.Scheme.Equals("ms-gamebarwidget", StringComparison.OrdinalIgnoreCase))
                return;

            var widgetArgs = args as XboxGameBarWidgetActivatedEventArgs;
            if (widgetArgs == null)
                return;

            string host = uri.Host;
            string widgetId = null;
            widgetId = uri.Host;
            if (string.IsNullOrEmpty(widgetId))
                widgetId = uri.AbsolutePath.TrimStart('/');
            Trace.WriteLine($"Widget OnActivated widgetId: {widgetId}");

            if (!widgetArgs.IsLaunchActivation)
            {
                // Handle reactivation or URI commands if needed.
                Trace.WriteLine($"Widget reactivated for host: {host}");
                return;
            }

            // --- New widget instance ---
            var rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;
            Window.Current.Content = rootFrame;

            Type targetPage = widgetId switch
            {
                "Tooth.XboxGameBarUI" => typeof(MainPage),
                "ColorRemaster.XboxGameBarUI" => typeof(ColorRemasterMainPage),
                _ => null
            };

            if (targetPage == null)
            {
                Trace.WriteLine($"Unknown widget widgetId: {widgetId}");
                return;
            }

            // Create the Game Bar widget object
            _xboxGameBarWidget = new XboxGameBarWidget(
                widgetArgs,
                Window.Current.CoreWindow,
                rootFrame);

            // Navigate to provide the widget to the page
            rootFrame.Navigate(targetPage, _xboxGameBarWidget);

            // Ensure backend is running
            if (!Backend.Instance.IsConnected)
                _ = Backend.LaunchBackend();

            // Hook visibility and close events
            _xboxGameBarWidget.VisibleChanged += XboxGameBarWidget_VisibleChanged;
            Window.Current.Closed += XboxGameBarWidgetWindow_Closed;

            // Initialize widget control
            _xboxGameBarWidgetControl = new XboxGameBarWidgetControl(_xboxGameBarWidget);
            _xboxGameBarWidgetControl.CreateActivationUri(
                "BassemNomany.ToothNClaw_ah2yj8jdj20z4",
                widgetId,
                string.Empty, string.Empty, string.Empty);

            // Finally activate the window
            Window.Current.Activate();
        }

        private void XboxGameBarWidget_VisibleChanged(XboxGameBarWidget sender, object e)
        {
            Trace.WriteLine($"[App.xaml.cs] XboxGameBarWidget_VisibleChanged");
            if (sender.Visible) // Only launch backend when it is visible and Backend is nsot r
            {
                if (!Backend.Instance.IsConnected)
                    _ = Backend.LaunchBackend();
            }
        }

        private void XboxGameBarWidgetWindow_Closed(object sender, Windows.UI.Core.CoreWindowEventArgs e)
        {
            Trace.WriteLine($"[App.xaml.cs] XboxGameBarWidgetWindow_Closed");
            _xboxGameBarWidget = null;
            Window.Current.Closed -= XboxGameBarWidgetWindow_Closed;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Trace.WriteLine($"[App.xaml.cs] OnLaunched");
            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // If we launch application manaually, not through gamebar, it will open MainPage
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Normally we
        /// wouldn't know if the app was being terminated or just suspended at this
        /// point. However, the app will never be suspended if Game Bar has an
        /// active widget connection to it, so if you see this call it's safe to
        /// cleanup any widget related objects. Keep in mind if all widgets are closed
        /// and you have a foreground window for your app, this could still result in 
        /// suspend or terminate. Regardless, it should always be safe to cleanup
        /// your widget related objects.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            _xboxGameBarWidget = null;

            deferral.Complete();
        }
    }
}

