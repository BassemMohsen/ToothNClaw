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

        private class PowerSettingSnapshot
        {
            public uint AC { get; set; }
            public uint DC { get; set; }
        }

        private const string SNAPSHOT_TAKEN_KEY = "PowerSnapshotTaken";
        private const string SNAPSHOT_KEY_PREFIX = "PowerSetting_";

        private Dictionary<string, PowerSettingSnapshot> _snapshot;
        private bool _snapshotTaken;

        public enum PowerButtonAction : int
        {
            DoNothing = 0,
            Sleep = 1,
            Hibernate = 2,
            Shutdown = 3,
            TurnOffDisplay = 4,
            Undefined = 5
        }
        public PowerPolicyController()
        {
            _snapshotTaken = SettingsManager.Get<bool>(SNAPSHOT_TAKEN_KEY);
            _snapshot = new Dictionary<string, PowerSettingSnapshot>();

            if (_snapshotTaken)
            {
                Console.WriteLine("[PowerPolicyController] Snapshot already exists, loading...");
                LoadSnapshot();
            }
        }


        private void SaveOriginalSetting(string name, Guid subgroup, Guid setting)
        {
            // Skip if snapshot already taken
            if (_snapshotTaken)
                return;

            var val = ReadValue(subgroup, setting);
            if (val.HasValue)
            {
                var snap = new PowerSettingSnapshot { AC = val.Value.ac, DC = val.Value.dc };
                _snapshot[name] = snap;
                SettingsManager.SetObject(SNAPSHOT_KEY_PREFIX + name, snap);
                Console.WriteLine($"[PowerPolicyController] Snapshot saved for {name}: AC={snap.AC}, DC={snap.DC}");
            }
        }

        private void FinalizeSnapshot()
        {
            if (!_snapshotTaken)
            {
                _snapshotTaken = true;
                SettingsManager.Set(SNAPSHOT_TAKEN_KEY, true);
                Console.WriteLine("[PowerPolicyController] Snapshot finalized and persisted.");
            }
        }

        private void LoadSnapshot()
        {
            string[] keys =
            {
                "HybridSleep", "WakeTimers", "AwayMode",
                "ModernStandbyNetwork", "ModernDisconnectedStandby",
                "PCIeASPM", "ProcessorIdle", "ProcessorThrottle"
            };

            foreach (var key in keys)
            {
                var snap = SettingsManager.GetObject<PowerSettingSnapshot>(SNAPSHOT_KEY_PREFIX + key);
                if (snap != null)
                {
                    _snapshot[key] = snap;
                    Console.WriteLine($"[PowerPolicyController] Loaded snapshot for {key}: AC={snap.AC}, DC={snap.DC}");
                }
            }
        }

        private static (uint ac, uint dc)? ReadValue(Guid subgroup, Guid setting)
        {
            var scheme = GetActiveScheme();
            if (scheme == Guid.Empty)
            {
                Console.WriteLine("[PowerPolicyController] No active power scheme found for read.");
                return null;
            }

            if (PowerReadACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out uint acVal) != 0)
            {
                Console.WriteLine($"[PowerPolicyController] Failed to read AC value for {setting}");
                return null;
            }

            if (PowerReadDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, out uint dcVal) != 0)
            {
                Console.WriteLine($"[PowerPolicyController] Failed to read DC value for {setting}");
                return null;
            }

            return (acVal, dcVal);
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
            if (result != 0) Console.WriteLine($"[PowerPolicyController] PowerWriteACValueIndex failed: {result} Subgroup {subgroup} Setting {setting} Value {value}");

            result = PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, value);
            if (result != 0) Console.WriteLine($"[PowerPolicyController] PowerWriteDCValueIndex failed: {result}  Subgroup {subgroup} Setting {setting} Value {value}");

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
            if (!_snapshotTaken)
            {
                Console.WriteLine("[PowerPolicyController] Taking initial power settings snapshot...");
                // Take snapshot before changing
                SaveOriginalSetting("HybridSleep", SUB_SLEEP, GUID_ALLOW_HYBRID_SLEEP);
                SaveOriginalSetting("WakeTimers", SUB_SLEEP, GUID_ALLOW_WAKE_TIMERS);
                SaveOriginalSetting("AwayMode", SUB_SLEEP, GUID_ALLOW_AWAY_MODE);
                SaveOriginalSetting("ModernStandbyNetwork", SUB_NONE, GUID_MODERN_STANDBY_NETWORK);
                SaveOriginalSetting("ModernDisconnectedStandby", SUB_NONE, GUID_MODERN_DISCONNECTED_STANDBY);
                SaveOriginalSetting("PCIeASPM", SUB_PCIEXPRESS, GUID_PCIEXPRESS_ASPM);
                SaveOriginalSetting("ProcessorIdle", SUB_PROCESSOR, GUID_IDLE_DISABLE);
                SaveOriginalSetting("ProcessorThrottle", SUB_PROCESSOR, GUID_PROCTHROTTLEMIN);
                FinalizeSnapshot();
                Console.WriteLine("[PowerPolicyController] Snapshot complete.");
            }

            Console.WriteLine("[PowerPolicyController] Applying new power settings...");
            DisableHybridSleep();
            DisableWakeTimers();
            SetMaxPCIePowerSavings();
            ConfigureProcessorIdleAndThrottle();
        }

        public void RestoreAll()
        {
            if (!_snapshotTaken)
            {
                Console.WriteLine("[PowerPolicyController] No persisted snapshot found, cannot restore.");
                return;
            }

            var scheme = GetActiveScheme();
            Console.WriteLine("[PowerPolicyController] Restoring power settings from persisted snapshot...");

            void Restore(string name, Guid subgroup, Guid setting)
            {
                if (_snapshot.TryGetValue(name, out var s))
                {
                    PowerWriteACValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, s.AC);
                    PowerWriteDCValueIndex(IntPtr.Zero, ref scheme, ref subgroup, ref setting, s.DC);
                    Console.WriteLine($"[PowerPolicyController] Restored {name}: AC={s.AC}, DC={s.DC}");
                }
            }

            Restore("HybridSleep", SUB_SLEEP, GUID_ALLOW_HYBRID_SLEEP);
            Restore("WakeTimers", SUB_SLEEP, GUID_ALLOW_WAKE_TIMERS);
            Restore("AwayMode", SUB_SLEEP, GUID_ALLOW_AWAY_MODE);
            Restore("ModernStandbyNetwork", SUB_NONE, GUID_MODERN_STANDBY_NETWORK);
            Restore("ModernDisconnectedStandby", SUB_NONE, GUID_MODERN_DISCONNECTED_STANDBY);
            Restore("PCIeASPM", SUB_PCIEXPRESS, GUID_PCIEXPRESS_ASPM);
            Restore("ProcessorIdle", SUB_PROCESSOR, GUID_IDLE_DISABLE);
            Restore("ProcessorThrottle", SUB_PROCESSOR, GUID_PROCTHROTTLEMIN);

            PowerSetActiveScheme(IntPtr.Zero, ref scheme);
        }

        public void SetPowerButtonAction(PowerButtonAction action)
        {
            Console.WriteLine($"[PowerPolicyController] Setting Power Button Action to {action}");

            WriteValue(SUB_POWER_BUTTON_LID, GUID_POWER_BUTTON_ACTION, (uint)action);
            WriteValue(SUB_POWER_BUTTON_LID, GUID_SLEEP_BUTTON_ACTION, (uint)action);
        }

        public PowerButtonAction GetPowerButtonAction()
        {
            Console.WriteLine("[PowerPolicyController] Reading Power Button Action...");
            var value = ReadValue(SUB_POWER_BUTTON_LID, GUID_POWER_BUTTON_ACTION);

            if (!value.HasValue)
            {
                Console.WriteLine("[PowerPolicyController] Failed to read Power Button Action.");
                return PowerButtonAction.Undefined;
            }

            // AC and DC values should be the same for this setting; use DC
            uint actionValue = value.Value.dc;

            if (Enum.IsDefined(typeof(PowerButtonAction), (int)actionValue))
                return (PowerButtonAction)actionValue;

            Console.WriteLine($"[PowerPolicyController] Unknown Power Button Action: {actionValue}");
            return PowerButtonAction.Undefined;
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
