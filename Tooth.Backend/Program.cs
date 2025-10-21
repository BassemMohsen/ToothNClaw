using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Foundation;
using Windows.Storage;
using Windows.System;
using WindowsInput;
using WindowsInput.Native;

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
        private static Handler handler;
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

            SettingsManager.Initialize();

            string packageSid;
            if (args.Length >= 1 && args[0].StartsWith("S-1-"))
                packageSid = args[0];
            else
                packageSid = ApplicationData.Current.LocalSettings.Values["PackageSid"] as string;

            var comm = new Communication(packageSid);
            handler = new Handler();

            handler.Register(comm);
            Task.Run(() => comm.Run()); // Run in background

            // Handle Controller combo shortcut listerner
            var comboListener = new XboxComboListener();

            comboListener.ComboPressed += () =>
            {
                Console.WriteLine("View + X pressed!");
                LaunchToothGameBar();
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

        private static void LaunchToothGameBarWidget()
        {
            handler.sendLaunchGameBarWidget();
        }

        private static void LaunchToothGameBar ()
        {

            // URIs doesn't work for gamebar, and neither does it work from power shell, which is very strange !
            // Is this a bug from Microsoft, because this page claims it works:
            // https://learn.microsoft.com/en-us/gaming/game-bar/api/xgb-widgetcontrol
            /*bool result = await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-gamebar:/activate/BassemNomany.ToothNClaw_ah2yj8jdj20z4_BassemNomany.ToothNClaw_Tooth.XboxGameBarUI_Tooth.XboxGameBarUI"));
            if(result)
            {
                Console.WriteLine("LaunchToothGameBar () successful!");
            } else
            {
                Console.WriteLine("LaunchToothGameBar () failed!");
            }*/

            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyDown(VirtualKeyCode.LWIN);
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.VK_G);
            inputSimulator.Keyboard.KeyUp(VirtualKeyCode.VK_G);
            inputSimulator.Keyboard.KeyUp(VirtualKeyCode.LWIN);
        }

    }
}
