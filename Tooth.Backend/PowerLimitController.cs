using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Tooth.Backend
{
    public class PowerLimitController : IDisposable
    {
        private bool disposedValue;

        protected string WmiScope { get; set; } = "root\\WMI";
        protected string WmiPath { get; set; } = "MSI_ACPI.InstanceName='ACPI\\PNP0C14\\0_0'";

        protected int WmiMajorVersion;
        protected int WmiMinorVersion;

        protected bool IsNewEC => WmiMajorVersion > 1;

        /// <summary>
        /// Checks WMI availability and retrieves version info.
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            byte iDataBlockIndex = 1;
            byte[] dataWMI = await WMI.GetAsync(WmiScope, WmiPath, "Get_WMI", iDataBlockIndex, 32);

            if (dataWMI.Length > 2)
            {
                WmiMajorVersion = dataWMI[1];
                WmiMinorVersion = dataWMI[2];
                Console.WriteLine($"[PowerLimitController] WMI Version: {WmiMajorVersion}.{WmiMinorVersion}");
                return true;
            }
            else
            {
                Console.WriteLine("[PowerLimitController] Could not read WMI version data (Get_WMI returned incomplete data).");
                return false;
            }
        }

        /// <summary>
        /// Sets sustained TDP (PL1) limit in watts.
        /// </summary>
        public async Task SetTDPLongSustainedLimitAsync(int limit)
        {
            if (!await InitializeAsync()) return;

            Console.WriteLine($"[PowerLimitController] Setting TDP Long (PL1) to {limit}W...");
            await SetCPUPowerLimitAsync(80, limit);
        }

        /// <summary>
        /// Sets short burst TDP (PL2) limit in watts.
        /// </summary>
        public async Task SetTDPShortBurstLimitAsync(int limit)
        {
            if (!await InitializeAsync()) return;

            Console.WriteLine($"[PowerLimitController] Setting TDP Short (PL2) to {limit}W...");
            await SetCPUPowerLimitAsync(81, limit);
        }

        /// <summary>
        /// Internal helper for writing TDP data to EC.
        /// </summary>
        private async Task SetCPUPowerLimitAsync(int iDataBlockIndex, int limit)
        {
            // iDataBlockIndex = 80 => Long (PL1)
            // iDataBlockIndex = 81 => Short (PL2)

            byte[] fullPackage = new byte[32];
            fullPackage[0] = (byte)iDataBlockIndex;
            fullPackage[1] = (byte)limit;

            var result = await WMI.SetAsync(WmiScope, WmiPath, "Set_Data", fullPackage);

            if (result != null)
                Console.WriteLine($"[PowerLimitController] Successfully set power limit (Index {iDataBlockIndex}) to {limit}W");
            else
                Console.WriteLine($"[PowerLimitController] Failed to set power limit (Index {iDataBlockIndex})");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
                disposedValue = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

