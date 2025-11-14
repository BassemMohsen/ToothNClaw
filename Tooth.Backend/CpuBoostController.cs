using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Tooth.Backend
{

    public enum SchedulingPolicyMode
    {
        AllCoresAuto,
        AllCoresPrefPCore,
        AllCoresPrefECore,
        OnlyPCore,
        OnlyECore
    }

    public readonly struct HeterogeneousPolicySet
    {
        public readonly uint Policy;
        public readonly uint ThreadPolicy;
        public readonly uint ShortThreadPolicy;

        public HeterogeneousPolicySet(uint policy, uint threadPolicy, uint shortThreadPolicy)
        {
            Policy = policy;
            ThreadPolicy = threadPolicy;
            ShortThreadPolicy = shortThreadPolicy;
        }
    }

    public static class SchedulingPolicyMappings
    {
        public static readonly Dictionary<SchedulingPolicyMode, HeterogeneousPolicySet> Map =
            new Dictionary<SchedulingPolicyMode, HeterogeneousPolicySet>
            {
            // Default Windows behavior
            {
                SchedulingPolicyMode.AllCoresAuto,
                new HeterogeneousPolicySet(
                    0U, // HETEROGENEOUS_POLICY = Default
                    5U, // THREAD = No preference
                    5U  // SHORT = No preference
                )
            },

            // Mixed cores but prefer P-cores
            {
                SchedulingPolicyMode.AllCoresPrefPCore,
                new HeterogeneousPolicySet(
                    1U, // Allow mixed, but scheduling hints matter
                    2U, // Prefer P
                    2U  // Prefer P
                )
            },

            // Mixed cores but prefer E-cores
            {
                SchedulingPolicyMode.AllCoresPrefECore,
                new HeterogeneousPolicySet(
                    1U, // Allow mixed
                    4U, // Prefer E
                    4U  // Prefer E
                )
            },

            // Lock to P-cores only
            {
                SchedulingPolicyMode.OnlyPCore,
                new HeterogeneousPolicySet(
                    3U, // FORCE P-cores only
                    1U, // Strongly P
                    1U  // Strongly P
                )
            },

            // Lock to E-cores only
            {
                SchedulingPolicyMode.OnlyECore,
                new HeterogeneousPolicySet(
                    2U, // FORCE E-cores only
                    3U, // Strongly E
                    3U  // Strongly E
                )
            },
            };
    }

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

        // CPU Boost setting
        private static readonly Guid BoostSettingGuidConst = new("be337238-0d82-4146-a960-4f3749d470c7");

        // Max frequency (MHz)
        private static readonly Guid PcoreMaxFreqGuidConst = new("75b0ae3f-bce0-45a7-8c89-c9611c25e100");  // PROCFREQMAX
        private static readonly Guid EcoreMaxFreqGuidConst = new("75b0ae3f-bce0-45a7-8c89-c9611c25e101");  // PROCFREQMAX1

        // Scheduling policy
        private static readonly Guid CPUSchedulePolicyGuidConst = new ("7f2f5cfa-f10c-4823-b5e1-e93ae85f46b5");
        public static Guid LongThreadSchedulePolicyGuidConst = new Guid("93b8b6dc-0698-4d1c-9ee4-0644e900c85d");
        public static Guid ShortThreadSchedulePolicyGuidConst = new Guid("bae08b81-2d5e-4688-ad6a-13243356654b");


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

        public void SetMaxPCoresFrequency(uint percent)
        {
            var schemeGuid = GetActiveScheme();
            var subgroupGuid = ProcessorGroupGuidConst;
            var settingGuid = PcoreMaxFreqGuidConst;

            PowerWriteACValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, percent);
            PowerWriteDCValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, percent);
            PowerSetActiveScheme(IntPtr.Zero, ref schemeGuid);
        }

        public uint GetMaxPCoresFrequency()
        {
            var schemeGuid = GetActiveScheme();
            var subgroupGuid = ProcessorGroupGuidConst;
            var settingGuid = PcoreMaxFreqGuidConst;

            PowerReadACValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, out uint val);
            return val;
        }

        public void SetMaxECoresFrequency(uint percent)
        {
            var schemeGuid = GetActiveScheme();
            var subgroupGuid = ProcessorGroupGuidConst;
            var settingGuid = EcoreMaxFreqGuidConst;

            PowerWriteACValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, percent);
            PowerWriteDCValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, percent);
            PowerSetActiveScheme(IntPtr.Zero, ref schemeGuid);
        }

        public uint GetMaxECoresFrequency()
        {
            var schemeGuid = GetActiveScheme();
            var subgroupGuid = ProcessorGroupGuidConst;
            var settingGuid = EcoreMaxFreqGuidConst;

            PowerReadACValueIndex(IntPtr.Zero, ref schemeGuid, ref subgroupGuid, ref settingGuid, out uint val);
            return val;
        }

        private void WritePowerValue(Guid scheme, Guid subgroup, Guid setting, uint value)
        {
            uint result;

            // Write AC value
            result = PowerWriteACValueIndex(IntPtr.Zero, ref scheme,
                                            ref subgroup, ref setting, value);
            if (result != 0)
                throw new InvalidOperationException($"PowerWriteACValueIndex failed: 0x{result:X}");

            // Write DC value
            result = PowerWriteDCValueIndex(IntPtr.Zero, ref scheme,
                                            ref subgroup, ref setting, value);
            if (result != 0)
                throw new InvalidOperationException($"PowerWriteDCValueIndex failed: 0x{result:X}");
        }

        public void RequestSchedulingPolicyMode(SchedulingPolicyMode mode)
        {
            // 1. Get current active power scheme
            if (PowerGetActiveScheme(IntPtr.Zero, out IntPtr activeSchemePtr) != 0)
                throw new InvalidOperationException("Failed to obtain active power scheme.");

            try
            {
                Guid activeScheme = Marshal.PtrToStructure<Guid>(activeSchemePtr);

                // Get value set for this mode
                var set = SchedulingPolicyMappings.Map[mode];

                // 2. Apply all 3 settings (AC/DC identical)
                WritePowerValue(activeScheme, ProcessorGroupGuidConst, CPUSchedulePolicyGuidConst, set.Policy);
                WritePowerValue(activeScheme, ProcessorGroupGuidConst, LongThreadSchedulePolicyGuidConst, set.ThreadPolicy);
                WritePowerValue(activeScheme, ProcessorGroupGuidConst, ShortThreadSchedulePolicyGuidConst, set.ShortThreadPolicy);

                // 3. Apply (write) scheme to system
                var result = PowerSetActiveScheme(IntPtr.Zero, ref activeScheme);
                if (result != 0)
                    throw new InvalidOperationException($"Failed to set active scheme: {result}");

                Console.WriteLine("User requested Core Parking Mode: {0}", mode);
            }
            finally
            {
                // Free pointer returned by PowerGetActiveScheme
                if (activeSchemePtr != IntPtr.Zero)
                    LocalFree(activeSchemePtr);
            }
        }

        public SchedulingPolicyMode getSchedulingPolicyMode()
        {
            return SchedulingPolicyMode.AllCoresAuto;
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

