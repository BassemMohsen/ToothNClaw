using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Tooth.Backend
{
    public static class GameSuspendController
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateToolhelp32Snapshot(uint dwFlags, uint th32ProcessID);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Process32First(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool Process32Next(IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtSuspendProcess(IntPtr processHandle);

        [DllImport("ntdll.dll", SetLastError = true)]
        private static extern uint NtResumeProcess(IntPtr processHandle);

        private const uint TH32CS_SNAPPROCESS = 0x00000002;
        private const uint PROCESS_SUSPEND_RESUME = 0x0800;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESSENTRY32
        {
            public uint dwSize;
            public uint cntUsage;
            public uint th32ProcessID;
            public IntPtr th32DefaultHeapID;
            public uint th32ModuleID;
            public uint cntThreads;
            public uint th32ParentProcessID;
            public int pcPriClassBase;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szExeFile;
        }

        // Whitelisted processes that should never be suspended
        private static readonly string[] WhitelistedProcesses =
        {
            "ApplicationFrameHost",
            "dwm",
            "explorer",
            "perfmon",
            "SystemSettings",
            "Taskmgr",
            "TextInputHost",
            "WinStore.App",
            "steamwebhelper",
            "EpicGamesLauncher",
            "Tooth",
            "WindowsTerminal",
        };

        private static bool IsProcessSuspended(Process process)
        {
            try
            {
                foreach (ProcessThread thread in process.Threads)
                {
                    if (thread.ThreadState == ThreadState.Wait &&
                        thread.WaitReason == ThreadWaitReason.Suspended)
                    {
                        // Found a suspended thread
                        return true;
                    }
                }
            }
            catch
            {
                // Process might have exited or is protected
            }

            return false;
        }

        private static bool IsWhitelisted(string processName)
        {
            return WhitelistedProcesses.Any(p =>
                string.Equals(p, processName, StringComparison.OrdinalIgnoreCase));
        }

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
                using (var process = Process.GetProcessById((int)processId))
                {
                    if (IsWhitelisted(process.ProcessName))
                    {
                        Console.WriteLine($"[GameSuspendController] Skipping whitelisted process: {process.ProcessName}");
                        return;
                    }

                    if (IsProcessSuspended(process))
                    {
                        Console.WriteLine($"[GameSuspendController] Process already suspended: {process.ProcessName}");
                        return;
                    }

                    Console.WriteLine($"[GameSuspendController] Suspending process: {process.ProcessName} ({processId})");
                    SuspendProcessTree(process.Id);
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
                using (var process = Process.GetProcessById((int)processId))
                {
                    if (IsWhitelisted(process.ProcessName))
                    {
                        Console.WriteLine($"[GameSuspendController] Skipping whitelisted process: {process.ProcessName}");
                        return;
                    }

                    Console.WriteLine($"[GameSuspendController] Resuming process: {process.ProcessName} ({processId})");
                    ResumeProcessTree(process.Id);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GameSuspendController] Failed to resume process: {ex.Message}");
            }
        }

        private static List<int> GetChildProcesses(int parentPid)
        {
            var childPids = new List<int>();
            IntPtr snapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
            if (snapshot == INVALID_HANDLE_VALUE)
                return childPids;

            try
            {
                PROCESSENTRY32 entry = new PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf(typeof(PROCESSENTRY32)) };
                if (!Process32First(snapshot, ref entry))
                    return childPids;

                do
                {
                    if (entry.th32ParentProcessID == parentPid)
                        childPids.Add((int)entry.th32ProcessID);
                }
                while (Process32Next(snapshot, ref entry));
            }
            finally
            {
                CloseHandle(snapshot);
            }

            return childPids;
        }

        public static void SuspendProcessTree(int pid)
        {
            if (pid == 0) return;

            SuspendOrResumeProcess(pid, suspend: true);

            foreach (var childPid in GetChildProcesses(pid))
                SuspendProcessTree(childPid);
        }

        public static void ResumeProcessTree(int pid)
        {
            if (pid == 0) return;

            SuspendOrResumeProcess(pid, suspend: false);

            foreach (var childPid in GetChildProcesses(pid))
                ResumeProcessTree(childPid);
        }


        private static void SuspendOrResumeProcess(int pid, bool suspend)
        {
            IntPtr hProc = OpenProcess(PROCESS_SUSPEND_RESUME, false, pid);
            if (hProc == IntPtr.Zero)
            {
                Console.WriteLine($"[ProcessTreeController] Cannot open PID {pid}. Error: {Marshal.GetLastWin32Error()}");
                return;
            }

            try
            {
                uint result = suspend ? NtSuspendProcess(hProc) : NtResumeProcess(hProc);
                Console.WriteLine($"[{(suspend ? "Suspend" : "Resume")}] PID {pid} success: {result == 0}");
            }
            finally
            {
                CloseHandle(hProc);
            }
        }

    }
}
