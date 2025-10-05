using System;
using System.Drawing;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Tooth.Backend
{
    internal static class Program
    {
        private static Mutex _mutex;

        [STAThread]
        static void Main()
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
            var comm = new Communication();
            var handler = new Handler();
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
