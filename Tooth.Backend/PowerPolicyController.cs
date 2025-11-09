using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Tooth.Backend
{
    public class PowerPolicyController : IDisposable
    {
        private bool disposedValue;

        // --- powrprof.dll native APIs ---
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr ActivePolicyGuid);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, ref Guid SchemeGuid);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerWriteACValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, uint AcValueIndex);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerWriteDCValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, uint DcValueIndex);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerReadACValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, out uint AcValueIndex);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerReadDCValueIndex(IntPtr RootPowerKey, ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid, ref Guid PowerSettingGuid, out uint DcValueIndex);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        // --- Common GUIDs ---
        private static readonly Guid SUB_SLEEP = new("238C9FA8-0AAD-41ED-83F4-97BE242C8F20");
        private static readonly Guid SUB_PROCESSOR = new("54533251-82BE-4824-96C1-47B60B740D00");
        private static readonly Guid SUB_PCIEXPRESS = new("501A4D13-42AF-4429-9FD1-A8218C268E20");
        private static readonly Guid SUB_NONE = new("fea3413e-7e05-4911-9a71-700331f1c294");
        //Power button and lid settings
        private static readonly Guid SUB_POWER_BUTTON_LID = new("4f971e89-eebd-4455-a8de-9e59040e7347");

        private static readonly Guid GUID_ALLOW_HYBRID_SLEEP = new("94ac6d29-73ce-41a6-809f-6363ba21b47e");
        private static readonly Guid GUID_ALLOW_AWAY_MODE = new("25dfa149-5dd1-4736-b5ab-e8a37b5b8187");
        private static readonly Guid GUID_ALLOW_WAKE_TIMERS = new("BD3B718A-0680-4D9D-8AB2-E1D2B4AC806D");
        private static readonly Guid GUID_MODERN_DISCONNECTED_STANDBY = new("68afb2d9-ee95-47a8-8f50-4115088073b1");
        private static readonly Guid GUID_MODERN_STANDBY_NETWORK = new("F15576E8-98B7-4186-B944-EAFA664402D9");
        private static readonly Guid GUID_PCIEXPRESS_ASPM = new("EE12F906-D277-404B-B6DA-E5FA1A576DF5");
        private static readonly Guid GUID_IDLE_DISABLE = new("5D76A2CA-E8C0-402F-A133-2158492D58AD");
        private static readonly Guid GUID_PROCTHROTTLEMIN = new("893DEE8E-2BEF-41E0-89C6-B55D0929964C");
        // Power Button Action GUID
        private static readonly Guid GUID_POWER_BUTTON_ACTION = new("7648efa3-dd9c-4e3e-b566-50f929386280");
        private static readonly Guid GUID_SLEEP_BUTTON_ACTION = new("96996bc0-ad50-47ec-923b-6f41874dd9eb");
        private static readonly Guid GUID_LID_SWITCH_CLOSE_ACTION = new("5ca83367-6e45-459f-a27b-476b1d01c936");

        public enum PowerButtonAction : int
        {
            DoNothing = 0,
            Sleep = 1,
            Hibernate = 2,
            Shutdown = 3,
            TurnOffDisplay = 4
        }
        private static Guid GetActiveScheme()
        {
            if (PowerGetActiveScheme(IntPtr.Zero, out IntPtr pGuid) != 0)
                return Guid.Empty;

            var schemeGuid = (Guid)Marshal.PtrToStructure(pGuid, typeof(Guid));
            LocalFree(pGuid);
            return schemeGuid;
        }

        private static void WriteValue(Guid subgroup, Guid setting, uint value)
        {
            var scheme = GetActiveScheme();
            if (scheme == Guid.Empty)
            {
                Console.WriteLine("[PowerPolicyController] No active power scheme found.");
                return;
            }

            uint result = PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            if (result != 0) Console.WriteLine($"[PowerPolicyController] PowerWriteACValueIndex failed: {result}");

            result = PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            if (result != 0) Console.WriteLine($"[PowerPolicyController] PowerWriteDCValueIndex failed: {result}");

            PowerSetActiveScheme(IntPtr.Zero, ref scheme);
        }

        public void DisableHybridSleep()
        {
            Console.WriteLine("[PowerPolicyController] Disable Hybrid Sleep");
            WriteValue(SUB_SLEEP, GUID_ALLOW_HYBRID_SLEEP, 0);
        }

        public void DisableWakeTimers()
        {
            Console.WriteLine("[PowerPolicyController] Disable Wake Timers and Modern Standby Network");
            WriteValue(SUB_SLEEP, GUID_ALLOW_WAKE_TIMERS, 0);
            WriteValue(SUB_SLEEP, GUID_ALLOW_AWAY_MODE, 0);
            WriteValue(SUB_NONE, GUID_MODERN_STANDBY_NETWORK, 0);
            WriteValue(SUB_NONE, GUID_MODERN_DISCONNECTED_STANDBY, 0);
        }

        public void SetMaxPCIePowerSavings()
        {
            Console.WriteLine("[PowerPolicyController] Set PCIe ASPM Maximum Power Savings");
            WriteValue(SUB_PCIEXPRESS, GUID_PCIEXPRESS_ASPM, 2);
        }

        public void ConfigureProcessorIdleAndThrottle()
        {
            Console.WriteLine("[PowerPolicyController] Processor: Allow Idle + Min Throttle = 0%");
            WriteValue(SUB_PROCESSOR, GUID_IDLE_DISABLE, 0);
            WriteValue(SUB_PROCESSOR, GUID_PROCTHROTTLEMIN, 0);
        }


        public void ApplyAll()
        {
            DisableHybridSleep();
            DisableWakeTimers();
            SetMaxPCIePowerSavings();
            ConfigureProcessorIdleAndThrottle();
        }

        public void SetPowerButtonAction(PowerButtonAction action)
        {
            Console.WriteLine($"[PowerPolicyController] Setting Power Button Action to {action}");

            WriteValue(SUB_POWER_BUTTON_LID, GUID_POWER_BUTTON_ACTION, (uint)action);
            WriteValue(SUB_POWER_BUTTON_LID, GUID_SLEEP_BUTTON_ACTION, (uint)action);
            WriteValue(SUB_POWER_BUTTON_LID, GUID_LID_SWITCH_CLOSE_ACTION, (uint)action);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
