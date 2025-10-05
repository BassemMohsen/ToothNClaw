using System;
using System.Diagnostics;
using System.ServiceModel.Channels;
using System.Text.RegularExpressions;


namespace Tooth.Backend
{
    public class CpuBoostController : IDisposable
    {
        public enum BoostMode
        {
            Disabled = 0,
            Enabled = 1,
            Aggressive = 2
        }

        private const string BoostSettingGuid = "be337238-0d82-4146-a960-4f3749d470c7";
        private const string ProcessorGroupGuid = "54533251-82be-4824-96c1-47b60b740d00";

        private bool disposedValue;

        public void SetBoostMode(BoostMode mode)
        {
            var scheme = GetActivePowerPlanGuid();
            RunCommand($"powercfg -setacvalueindex {scheme} {ProcessorGroupGuid} {BoostSettingGuid} {(int)mode}");
            RunCommand($"powercfg -S {scheme}"); // Apply

            Trace.WriteLine($"SetBoostMode {mode}");

        }

        public BoostMode? GetBoostMode()
        {
            var scheme = GetActivePowerPlanGuid();
            string cmd = $"powercfg -query {scheme} {ProcessorGroupGuid} {BoostSettingGuid}";
            string output = RunCommandWithOutput(cmd);

            var match = Regex.Match(output, @"Current AC Power Setting Index: 0x(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int value))
            {
                Trace.WriteLine($"GetBoostMode {value}");
                return (BoostMode)value;
            }

            return null;
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

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~CpuBoostController()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

}