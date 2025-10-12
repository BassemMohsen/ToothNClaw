using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;


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

        private const string BoostSettingGuid = "be337238-0d82-4146-a960-4f3749d470c7";
        private const string ProcessorGroupGuid = "54533251-82be-4824-96c1-47b60b740d00";

        private bool disposedValue;

        public void SetBoostMode(BoostMode mode)
        {
            var scheme = GetActivePowerPlanGuid();
            RunCommand($"powercfg -setacvalueindex {scheme} {ProcessorGroupGuid} {BoostSettingGuid} {(int)mode}");
            RunCommand($"powercfg -setdcvalueindex {scheme} {ProcessorGroupGuid} {BoostSettingGuid} {(int)mode}");
            
            RunCommand($"powercfg -S {scheme}"); // Apply

            Trace.WriteLine($"SetBoostMode {mode}");

        }

        public BoostMode GetBoostMode()
        {
            try
            {
                var scheme = GetActivePowerPlanGuid();
                if (scheme == null)
                    return BoostMode.UnsupportedAndHidden;

                string cmd = $"powercfg -query {scheme} {ProcessorGroupGuid} {BoostSettingGuid}";
                string output = RunCommandWithOutputSafe(cmd);

                // Check if the output contains "No such power setting"
                if (string.IsNullOrWhiteSpace(output) ||
                    output.IndexOf("No such power setting", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Trace.WriteLine("CPU Boost is unsupported or hidden");
                    return BoostMode.UnsupportedAndHidden;
                }

                var matchAC = Regex.Match(output, @"Current AC Power Setting Index: 0x(\d+)");
                var matchDC = Regex.Match(output, @"Current DC Power Setting Index: 0x(\d+)");

                if (matchAC.Success && int.TryParse(matchAC.Groups[1].Value, out int acValue))
                    Trace.WriteLine($"GetBoostMode AC {acValue}");

                if (matchDC.Success && int.TryParse(matchDC.Groups[1].Value, out int dcValue))
                    Trace.WriteLine($"GetBoostMode DC {dcValue}");

                // Prefer AC value if available, else DC
                if (matchAC.Success)
                    return (BoostMode)int.Parse(matchAC.Groups[1].Value);
                if (matchDC.Success)
                    return (BoostMode)int.Parse(matchDC.Groups[1].Value);

                // If parsing failed, treat as unsupported/hidden
                return BoostMode.UnsupportedAndHidden;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failed to get CPU boost mode: {ex.Message}");
                return BoostMode.UnsupportedAndHidden;
            }
        }


        private string GetActivePowerPlanGuid()
        {
            var output = RunCommandWithOutput("powercfg /getactivescheme");
            var match = Regex.Match(output, @"GUID: ([a-fA-F0-9\-]+)");
            return match.Success ? match.Groups[1].Value : null;
        }

        private string RunCommandWithOutput(string command)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            p.Start();
            return p.StandardOutput.ReadToEnd();
        }

        // Helper wrapper for safe command execution
        private string RunCommandWithOutputSafe(string command)
        {
            try
            {
                return RunCommandWithOutput(command);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Command failed: {command}, Error: {ex.Message}");
                return string.Empty;
            }
        }

        private void RunCommand(string command)
        {
            using var p = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };
            p.Start();

            string  output = p.StandardOutput.ReadToEnd();
            string  error = p.StandardError.ReadToEnd();

            Trace.WriteLine($"RunCommand Output {output}");
            Trace.WriteLine($"RunCommand Error {error}");

            p.WaitForExit();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}