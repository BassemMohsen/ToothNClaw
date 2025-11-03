using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using System.Threading.Tasks;

namespace Tooth.Backend
{

    public static class WMI
    {
        public static async Task<bool> ExistsAsync(string scope, FormattableString query, CancellationToken token = default)
        {
            try
            {
                string queryFormatted = query.ToString(WMIPropertyValueFormatter.Instance);
                return await Task.Run(() =>
                {
                    using var mos = new ManagementObjectSearcher(scope, queryFormatted);
                    var results = mos.Get();
                    token.ThrowIfCancellationRequested();
                    return results.Cast<ManagementObject>().Any();
                }, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WMI.ExistsAsync] {ex.Message}");
                return false;
            }
        }

        public static IDisposable Listen(string scope, FormattableString query, Action<PropertyDataCollection> handler)
        {
            string queryFormatted = query.ToString(WMIPropertyValueFormatter.Instance);
            var watcher = new ManagementEventWatcher(scope, queryFormatted);
            watcher.EventArrived += (_, e) => handler(e.NewEvent.Properties);
            watcher.Start();
            return new LambdaDisposable(() =>
            {
                watcher.Stop();
                watcher.Dispose();
            });
        }

        public static async Task<IEnumerable<T>> ReadAsync<T>(
            string scope,
            FormattableString query,
            Func<PropertyDataCollection, T> converter,
            CancellationToken token = default)
        {
            try
            {
                string queryFormatted = query.ToString(WMIPropertyValueFormatter.Instance);
                return await Task.Run(() =>
                {
                    using var mos = new ManagementObjectSearcher(scope, queryFormatted);
                    var results = mos.Get();
                    token.ThrowIfCancellationRequested();
                    return results.Cast<ManagementObject>().Select(mo => converter(mo.Properties)).ToList();
                }, token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WMI.ReadAsync] {ex.Message}");
                return Enumerable.Empty<T>();
            }
        }

        public static async Task<ManagementBaseObject?> SetAsync(
            string scope,
            string path,
            string methodName,
            byte[] fullPackage,
            CancellationToken token = default)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var managementObject = new ManagementObject(scope, path, null);

                    if (managementObject == null)
                        throw new InvalidOperationException("WMI ManagementObject is null");

                    ManagementBaseObject inParams = null;
                    ManagementBaseObject inParamsData = null;
                    bool parametersAvailable = false;

                    try
                    {
                        inParams = managementObject.GetMethodParameters(methodName);
                        inParamsData = inParams["Data"] as ManagementBaseObject;
                        parametersAvailable = (inParams != null && inParamsData != null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WMI.SetAsync] Failed to get inParams  GetMethodParameters {ex.Message}");
                    }

                    // If the "Data" parameter was not obtained, try the fallback method "Get_WMI"
                    if (!parametersAvailable)
                    {
                        try
                        {
                            inParams = managementObject.InvokeMethod("Get_WMI", null, null);
                            inParamsData = inParams["Data"] as ManagementBaseObject;
                        }
                        catch (ManagementException mex)
                        {
                            Console.WriteLine($"[WMI.SetAsync] Failed to get inParams  ManagementException Get_WMI {mex.Message}");

                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[WMI.SetAsync] Failed to get inParams  ManagementException Get_WMI {ex.Message}");

                        }
                    }

                    // Set the "Bytes" property of the "Data" parameter to the full package
                    inParamsData.SetPropertyValue("Bytes", fullPackage);
                    inParams.SetPropertyValue("Data", inParamsData);

                    token.ThrowIfCancellationRequested();
                    return managementObject.InvokeMethod(methodName, inParams, new InvokeMethodOptions());
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WMI.SetAsync] {ex.Message}");
                    return null;
                }
            }, token).ConfigureAwait(false);
        }

        public static async Task<byte[]> GetAsync(
            string scope,
            string path,
            string methodName,
            byte iDataBlockIndex,
            int length,
            CancellationToken token = default)
        {
            byte[] emptyResult = new byte[length];
            try
            {
                byte[] fullPackage = new byte[32];
                fullPackage[0] = iDataBlockIndex;

                var result = await SetAsync(scope, path, methodName, fullPackage, token).ConfigureAwait(false);
                if (result == null)
                {
                    Console.WriteLine($"[WMI.GetAsync] Failed to SetAsync returnign null result !");
                    return emptyResult;
                }

                ManagementBaseObject dataOut = result["Data"] as ManagementBaseObject;
                if (dataOut == null)
                {
                    Console.WriteLine($"WMI Call failed at result[\"Data\"]: [scope={scope}, path={path}, methodName={methodName}, iDataBlockIndex={iDataBlockIndex}, length={length}]");

                    return emptyResult;
                }

                byte[] outBytes = dataOut["Bytes"] as byte[];
                if (outBytes == null || outBytes.Length < 2)
                {
                    Console.WriteLine("WMI Call failed at dataOut[\"Bytes\"]: [scope={scope}, path={path}, methodName={methodName}, iDataBlockIndex={iDataBlockIndex}, length={length}]");

                    return emptyResult;
                }

                byte flag = outBytes[0];
                if (flag != 1) {
                    Console.WriteLine("WMI Call failed at read flag is not successful");

                    return emptyResult;
                }

                byte[] resultData = new byte[outBytes.Length - 1];
                Array.Copy(outBytes, 1, resultData, 0, resultData.Length);
                return resultData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WMI.GetAsync] {ex.Message}");
                return emptyResult;
            }
        }

        public class LambdaDisposable : IDisposable
        {
            private readonly Action _disposeAction;
            public LambdaDisposable(Action disposeAction) => _disposeAction = disposeAction;
            public void Dispose() => _disposeAction?.Invoke();
        }

        public class WMIPropertyValueFormatter : IFormatProvider, ICustomFormatter
        {
            public static readonly WMIPropertyValueFormatter Instance = new();
            public object GetFormat(Type formatType) => formatType == typeof(ICustomFormatter) ? this : null!;
            public string Format(string format, object arg, IFormatProvider formatProvider)
                => arg?.ToString()?.Replace("\\", "\\\\") ?? string.Empty;
        }
    }


}
