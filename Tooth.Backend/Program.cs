using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Storage;
using Windows.System;

namespace Tooth.Backend
{
    internal static class Program
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static Mutex _mutex;
        const string PROGRAM_NAME = "ToothNClaw.Service";

        [STAThread]
        static void Main(string[] args)
        {
            // Hide console window only in Release mode
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);

            _mutex = new Mutex(true, "Tooth.Backend");
            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                //MessageBox.Show("Tooth Backend is already running.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Start your backend communication
            Console.WriteLine($"{PROGRAM_NAME}");
            Console.WriteLine($"[Program] Started with Argument {args[0]}");

            string packageSid;
            if (args.Length >= 1 && args[0].StartsWith("S-1-"))
                packageSid = args[0];
            else
                packageSid = ApplicationData.Current.LocalSettings.Values["PackageSid"] as string;

            var comm = new Communication(packageSid);
            var handler = new Handler();

            handler.Register(comm);
            Task.Run(() => comm.Run()); // Run in background

            // Handle Controller combo shortcut listerner
            var comboListener = new XboxComboListener();

            comboListener.ComboPressed += () =>
            {
                Console.WriteLine("View + A pressed!");
                LaunchToothGameBarWidget();
            };

            comboListener.ComboReleased += () =>
            {
                Console.WriteLine("View + A released.");
            };

            comboListener.Start();


            // Run hidden message loop to keep app alive
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayAppContext());
        }

        public class TrayAppContext : ApplicationContext
        {
            private NotifyIcon trayIcon;

            public TrayAppContext()
            {
                trayIcon = new NotifyIcon()
                {
                    Icon = Properties.Resources.tooth,
                    Text = "ToothNClaw Service",
                    Visible = true,
                    ContextMenuStrip = new ContextMenuStrip()
                    {
                        Items = {
                        new ToolStripMenuItem("Show/Hide", null, OnOpen),
                        new ToolStripMenuItem("Exit", null, OnExit)
                    }
                    }
                };

                // Handle double-click to toggle show/hide
                trayIcon.DoubleClick += OnOpen;
            }

            void OnOpen(object sender, EventArgs e)
            {

                var handle = GetConsoleWindow();
                if (handle == IntPtr.Zero)
                    return;
                // Check visibility
                bool isVisible = IsWindowVisible(handle);

                if (isVisible)
                {
                    ShowWindow(handle, SW_HIDE);
                    Console.WriteLine("Console hidden.");
                }
                else
                {
                    ShowWindow(handle, SW_SHOW);
                    Console.WriteLine("Console shown.");
                }
            }

            void OnExit(object sender, EventArgs e)
            {
                trayIcon.Visible = false;
                Application.Exit();
            }
        }

        private static void OnExitClicked(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static async void LaunchToothGameBarWidget()
        {
            string uriString = "ms-gamebar://launch/activate/Tooth.Package_c7kwspyh8mqh4_Tooth.App_Tooth.XboxGameBarUI";
            var uri = new Uri(uriString);
            try
            {
                Console.WriteLine($"Open Game Bar: {uri}");

                bool success = await Launcher.LaunchUriAsync(uri);

                if (!success)
                {
                    Console.WriteLine($"Failed to launch widget {uriString}.");
                }

                /*Process.Start(new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                });*/
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open Game Bar: {ex.Message}");
            }
        }

        private static void HideGameBar()
        {
            string uri = "ms-gamebar://hide";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = uri,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to open Game Bar: {ex.Message}");
            }
        }
    }
}
