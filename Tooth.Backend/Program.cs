using System;
using System.Drawing;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;
using Windows.Storage;



namespace Tooth.Backend
{
    internal static class Program
    {
        private static Mutex _mutex;
        const string PROGRAM_NAME = "Tooth.Backend";

        [STAThread]
        static void Main(string[] args)
        {
            _mutex = new Mutex(true, "Tooth.Backend");
            if (!_mutex.WaitOne(TimeSpan.Zero, true))
            {
                MessageBox.Show("Tooth Backend is already running.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Check admin privileges
            var principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Tooth Backend must run as Administrator.", "Permission", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Start your backend communication
            Console.WriteLine($"{PROGRAM_NAME}");

            string packageSid;
            if (args.Length >= 1 && args[0].StartsWith("S-1-"))
                packageSid = args[0];
            else
                packageSid = ApplicationData.Current.LocalSettings.Values["PackageSid"] as string;

            var comm = new Communication(packageSid);
            var handler = new Handler(
                new AutoStart(PROGRAM_NAME,
                new Microsoft.Win32.TaskScheduler.ExecAction(
                    Assembly.GetEntryAssembly().Location, packageSid
                    )));

            handler.Register(comm);
            Task.Run(() => comm.Run()); // Run in background

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
                    Text = "Tooth Backend",
                    Visible = true,
                    ContextMenuStrip = new ContextMenuStrip()
                    {
                        Items = {
                        new ToolStripMenuItem("Open", null, OnOpen),
                        new ToolStripMenuItem("Exit", null, OnExit)
                    }
                    }
                };
            }

            void OnOpen(object sender, EventArgs e)
            {
                MessageBox.Show("App opened!");
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
    }
}
