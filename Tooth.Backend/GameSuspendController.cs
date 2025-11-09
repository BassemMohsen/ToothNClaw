using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tooth.Backend
{
    public static class GameSuspendController
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtSuspendProcess(IntPtr processHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtResumeProcess(IntPtr processHandle);

        public static void SuspendForegroundApp()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
            {
                Console.WriteLine("[GameSuspendController] No foreground window detected.");
                return;
            }

            GetWindowThreadProcessId(hwnd, out uint processId);

            try
            {
                var process = Process.GetProcessById((int)processId);
                using (process)
                {
                    Console.WriteLine($"[GameSuspendController] Suspending process: {process.ProcessName} ({processId})");
                    NtSuspendProcess(process.Handle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameSuspendController] Failed to suspend process: {ex.Message}");
            }
        }

        public static void ResumeForegroundApp()
        {
            IntPtr hwnd = GetForegroundWindow();
            if (hwnd == IntPtr.Zero)
            {
                Console.WriteLine("[GameSuspendController] No foreground window detected.");
                return;
            }

            GetWindowThreadProcessId(hwnd, out uint processId);

            try
            {
                var process = Process.GetProcessById((int)processId);
                using (process)
                {
                    Console.WriteLine($"[GameSuspendController] Resuming process: {process.ProcessName} ({processId})");
                    NtResumeProcess(process.Handle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameSuspendController] Failed to resume process: {ex.Message}");
            }
        }
    }
}
