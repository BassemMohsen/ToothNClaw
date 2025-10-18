using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.IO;
using System.Linq;

namespace Tooth.Backend
{
    public static class RTSSManager
    {
        private const string RTSS_EXE_NAME = "RTSS.exe";
        private const string RTSS_PROCESS_NAME = "RTSS";
        private const string DEFAULT_RTSS_PATH = @"C:\Program Files (x86)\RivaTuner Statistics Server\RTSS.exe";

        private const string GLOBAL_PROFILE = "Global";
        private const string DLL_NAME = "RTSSHooks64.dll";

        [DllImport(DLL_NAME)]
        public static extern void LoadProfile(string profile = GLOBAL_PROFILE);

        [DllImport(DLL_NAME)]
        public static extern void SaveProfile(string profile = GLOBAL_PROFILE);

        [DllImport(DLL_NAME)]
        public static extern void ResetProfile(string profile = GLOBAL_PROFILE);

        [DllImport(DLL_NAME)]
        public static extern void UpdateProfiles();

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi)]
        public static extern bool GetProfileProperty(string propertyName, nint value, uint size);

        [DllImport(DLL_NAME, CharSet = CharSet.Ansi)]
        public static extern bool SetProfileProperty(string propertyName, nint value, uint size);

        // RTSS GUI window class for message posting
        private const string RTSS_CLASS_NAME = "RTSSHooksWindowClass";
        //private const uint WM_RTSSEVENT = 0xBEEF; // Custom RTSS message
        private const uint WM_RTSSEVENT = 0x8000 + 100;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        // Property name for framerate limit in RTSS
        private const string PROP_FRAMERATE_LIMIT = "FramerateLimit";

        public static bool EnsureRunning()
        {
            // 1️⃣ Check if RTSS process is already running
            Process existing = Process.GetProcessesByName(RTSS_PROCESS_NAME).FirstOrDefault();
            if (existing != null)
            {
                // Just accessing Id or ProcessName is safe
                int pid = existing.Id;
                Console.WriteLine($"RTSS is already running (PID {pid}).");
                return true;
            }

            // 2️⃣ Try to locate the RTSS executable
            string path = DEFAULT_RTSS_PATH;
            if (!File.Exists(path))
            {
                // Optional: search the default install folder
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
                string candidate = Path.Combine(programFiles, "RivaTuner Statistics Server", "RTSS.exe");
                if (File.Exists(candidate))
                    path = candidate;
                else
                {
                    Console.WriteLine("RTSS executable not found. Please install RTSS first.");
                    return false;
                }
            }

            // 3️⃣ Start RTSS silently (minimized, no UI)
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    WorkingDirectory = Path.GetDirectoryName(path),
                    UseShellExecute = true,
                    CreateNoWindow = true
                };
                Process.Start(startInfo);
                Console.WriteLine("Starting RTSS...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to start RTSS: {ex.Message}");
                return false;
            }

            // 4️⃣ Wait until RTSS creates its main window
            for (int i = 0; i < 20; i++)
            {
                IntPtr hWnd = FindWindow(RTSS_CLASS_NAME, null);
                if (hWnd != IntPtr.Zero)
                {
                    Console.WriteLine("RTSS is ready.");
                    return true;
                }
                Thread.Sleep(500);
            }

            Console.WriteLine("RTSS did not become ready in time.");
            return false;
        }

        public static bool SetGlobalFramerateLimit(int fps)
        {
            if (fps < 0)
            {
                Console.WriteLine("Framerate cannot be negative.");
                return false;
            }

            // Load global profile
            LoadProfile(GLOBAL_PROFILE);

            // Allocate unmanaged memory for the FPS value
            IntPtr valPtr = Marshal.AllocHGlobal(sizeof(uint));
            Marshal.WriteInt32(valPtr, unchecked((int)(uint)fps));
            // Write property
            bool ok = SetProfileProperty(PROP_FRAMERATE_LIMIT, valPtr, sizeof(uint));

            Marshal.FreeHGlobal(valPtr);

            if (!ok)
            {
                Console.WriteLine("Failed to set FramerateLimit property.");
                return false;
            }

            // Save and apply changes
            SaveProfile(GLOBAL_PROFILE);
            UpdateProfiles();

            // Notify RTSS to reload
            IntPtr hWnd = FindWindow(null, "RTSS");
            if (hWnd == IntPtr.Zero)
                hWnd = FindWindow(null, "RivaTuner Statistics Server");

            if (hWnd != IntPtr.Zero)
            {
                PostMessage(hWnd, WM_RTSSEVENT, IntPtr.Zero, IntPtr.Zero);
                Console.WriteLine($"RTSS FindWindow handle is posted event to update.");
            }
            else
            {
                Console.WriteLine($"RTSS FindWindow handle is not found.");
            }

                Console.WriteLine($"Set RTSS global framerate limit to {fps} FPS.");
            return ok;
        }

        public static int GetGlobalFramerateLimit()
        {
            LoadProfile(GLOBAL_PROFILE);

            IntPtr valPtr = Marshal.AllocHGlobal(sizeof(uint));
            bool ok = GetProfileProperty(PROP_FRAMERATE_LIMIT, valPtr, sizeof(uint));

            Console.WriteLine(ok
                ? $"Current RTSS global framerate limit is read correctly: {Marshal.ReadInt32(valPtr)} FPS."
                : "Failed to get FramerateLimit property.");

            int fps = ok ? Marshal.ReadInt32(valPtr) : 0;
            Marshal.FreeHGlobal(valPtr);

            return fps;
        }

        public static bool IsRTTSRunning()
        {
            try
            {
                return Process.GetProcessesByName(RTSS_PROCESS_NAME).Any();
            }
            catch
            {
                return false;
            }
        }
    }
}