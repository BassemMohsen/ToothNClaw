using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tooth.Backend
{
    public class CpuBoostController : IDisposable
    {
        public enum BoostMode
        {
            UnsupportedAndHidden = -1,
            Disabled = 0,
            Enabled = 1,
            Aggressive = 2,
            EfficientEnabled = 3,
            EfficientAggressive = 4,
            AggressiveAtGuaranteed = 5,
            EfficientAggressiveAtGuaranteed = 6
        }

        private static readonly Guid ProcessorGroupGuidConst = new("54533251-82be-4824-96c1-47b60b740d00");
        private static readonly Guid BoostSettingGuidConst = new("be337238-0d82-4146-a960-4f3749d470c7");

        private bool disposedValue;

        // Import native Power Management APIs
        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerGetActiveScheme(IntPtr UserRootPowerKey, out IntPtr ActivePolicyGuid);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerSetActiveScheme(IntPtr UserRootPowerKey, ref Guid SchemeGuid);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerWriteACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint AcValueIndex);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerWriteDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            uint DcValueIndex);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerReadACValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint AcValueIndex);

        [DllImport("powrprof.dll", SetLastError = true)]
        private static extern uint PowerReadDCValueIndex(
            IntPtr RootPowerKey,
            ref Guid SchemeGuid,
            ref Guid SubGroupOfPowerSettingsGuid,
            ref Guid PowerSettingGuid,
            out uint DcValueIndex);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        public void SetBoostMode(BoostMode mode)
        {
            var schemeGuid = GetActiveScheme();
            if (schemeGuid == Guid.Empty)
            {
                Trace.WriteLine("No active power scheme found.");
                return;
            }

            // Make local copies so we can pass them by ref
            Guid subgroupGuid = ProcessorGroupGuidConst;
            Guid settingGuid = BoostSettingGuidConst;

            uint modeValue = (uint)mode;
            uint result;

            result = PowerWriteACValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, modeValue);
            if (result != 0)
                Trace.WriteLine($"PowerWriteACValueIndex failed: {result}");

            result = PowerWriteDCValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, modeValue);
            if (result != 0)
                Trace.WriteLine($"PowerWriteDCValueIndex failed: {result}");

            result = PowerSetActiveScheme(IntPtr.Zero, ref schemeGuid);
            if (result != 0)
                Trace.WriteLine($"PowerSetActiveScheme failed: {result}");

            Trace.WriteLine($"SetBoostMode to {mode}");
        }

        public BoostMode GetBoostMode()
        {
            try
            {
                var schemeGuid = GetActiveScheme();
                if (schemeGuid == Guid.Empty)
                    return BoostMode.UnsupportedAndHidden;

                // Make local copies for ref
                Guid subgroupGuid = ProcessorGroupGuidConst;
                Guid settingGuid = BoostSettingGuidConst;

                uint acValue = 0, dcValue = 0;
                bool acOk = PowerReadACValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, out acValue) == 0;
                bool dcOk = PowerReadDCValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, out dcValue) == 0;

                if (acOk)
                {
                    Trace.WriteLine($"GetBoostMode (AC): {acValue}");
                    return (BoostMode)acValue;
                }
                if (dcOk)
                {
                    Trace.WriteLine($"GetBoostMode (DC): {dcValue}");
                    return (BoostMode)dcValue;
                }

                Trace.WriteLine("CPU Boost mode not supported or hidden.");
                return BoostMode.UnsupportedAndHidden;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to read boost mode: {ex.Message}");
                return BoostMode.UnsupportedAndHidden;
            }
        }

        private static Guid GetActiveScheme()
        {
            if (PowerGetActiveScheme(IntPtr.Zero, out IntPtr pGuid) != 0)
                return Guid.Empty;

            var schemeGuid = (Guid)Marshal.PtrToStructure(pGuid, typeof(Guid));
            LocalFree(pGuid);
            return schemeGuid;
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

