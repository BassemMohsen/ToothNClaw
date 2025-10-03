using IntelAPUSettings.Backend.Libraries;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace IntelAPUSettings.Backend
{
    public enum IntelAdjError
    {
        None = 0,
        FamilyUnsupported = IntelAdjNative.ADJ_ERR_FAM_UNSUPPORTED, // -1
        SmuTimeout = IntelAdjNative.ADJ_ERR_SMU_TIMEOUT,      // -2
        SmuUnsupported = IntelAdjNative.ADJ_ERR_SMU_UNSUPPORTED,  // -3
        SmuRejected = IntelAdjNative.ADJ_ERR_SMU_REJECTED,     // -4
        MemoryAccess = IntelAdjNative.ADJ_ERR_MEMORY_ACCESS,    // -5
        UnknownNegative = int.MinValue
    }

    public sealed class IntelAdjException : Exception
    {
        public IntelAdjError Error { get; }
        public int RawCode { get; }
        public IntelAdjException(IntelAdjError error, int raw, string message = null)
            : base(message ?? $"IntelAdj error: {error} (code {raw})")
        {
            Error = error;
            RawCode = raw;
        }

        public static IntelAdjError FromReturnCode(int rc)
        {
            if (Enum.IsDefined(typeof(IntelAdjError), rc))
                return (IntelAdjError)rc;
            else if (rc < 0)
                return IntelAdjError.UnknownNegative;
            else
                return IntelAdjError.None;
        }

        public static void ThrowIfError(int rc, [CallerMemberName] string api = null)
        {
            if (rc >= 0) return;
            var err = FromReturnCode(rc);
            throw new IntelAdjException(err, rc, $"Call '{api}' failed: {err} ({rc})");
        }
    }

    public sealed class IntelAdj : IDisposable
    {
        private readonly object _lock = new object();
        private IntPtr _handle;
        private bool _disposed;

        public intel_family Family { get; }
        public int BiosIfVersion { get; }

        private IntelAdj(IntPtr handle)
        {
            _handle = handle;
            Family = IntelAdjNative.get_cpu_family(handle);
            BiosIfVersion = IntelAdjNative.get_bios_if_ver(handle);
        }

        public static IntelAdj Open(bool initTable = true)
        {
            var h = IntelAdjNative.init_inteladj();
            if (h == IntPtr.Zero)
                throw new IntelAdjException(IntelAdjError.MemoryAccess, (int)IntelAdjError.UnknownNegative, "init_inteladj returned null.");

            try
            {
                var fam = IntelAdjNative.get_cpu_family(h);
                if (fam == intel_family.FAM_UNKNOWN || fam == intel_family.FAM_END)
                {
                    IntelAdjNative.cleanup_inteladj(h);
                    throw new IntelAdjException(IntelAdjError.FamilyUnsupported, IntelAdjNative.ADJ_ERR_FAM_UNSUPPORTED, $"Unsupported CPU family: {fam}");
                }

                var client = new IntelAdj(h);
                if (initTable)
                {
                    var rc = IntelAdjNative.init_table(h);
                    IntelAdjException.ThrowIfError(rc);
                }
                return client;
            }
            catch
            {
                try { IntelAdjNative.cleanup_inteladj(h); } catch { /* ignored */ }
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            lock (_lock)
            {
                if (_disposed) return;
                if (_handle != IntPtr.Zero)
                {
                    IntelAdjNative.cleanup_inteladj(_handle);
                    _handle = IntPtr.Zero;
                }
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~IntelAdj() { Dispose(); }

        private void EnsureNotDisposed()
        {
            if (_disposed || _handle == IntPtr.Zero)
                throw new ObjectDisposedException(nameof(IntelAdj));
        }

        public void RefreshTable()
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                var rc = IntelAdjNative.refresh_table(_handle);
                IntelAdjException.ThrowIfError(rc);
            }
        }

        #region Properties
        public float StapmLimit_W
        {
            get => WithLock(() => IntelAdjNative.get_stapm_limit(_handle));
            set => Call(() => IntelAdjNative.set_stapm_limit(_handle, checked((uint)Math.Round(value * 1000))));
        }

        public float FastLimit_W
        {
            get => WithLock(() => IntelAdjNative.get_fast_limit(_handle));
            set => Call(() => IntelAdjNative.set_fast_limit(_handle, checked((uint)Math.Round(value * 1000))));
        }

        public float SlowLimit_W
        {
            get => WithLock(() => IntelAdjNative.get_slow_limit(_handle));
            set => Call(() => IntelAdjNative.set_slow_limit(_handle, checked((uint)Math.Round(value * 1000))));
        }

        public float StapmTime_s
        {
            get => WithLock(() => IntelAdjNative.get_stapm_time(_handle));
            set => Call(() => IntelAdjNative.set_stapm_time(_handle, checked((uint)Math.Round(value))));
        }

        public float SlowTime_s
        {
            get => WithLock(() => IntelAdjNative.get_slow_time(_handle));
            set => Call(() => IntelAdjNative.set_slow_time(_handle, checked((uint)Math.Round(value))));
        }

        public float TctlTemp_C
        {
            get => WithLock(() => IntelAdjNative.get_tctl_temp(_handle));
            set => Call(() => IntelAdjNative.set_tctl_temp(_handle, checked((uint)Math.Round(value))));
        }

        public float MinGfxClk_MHz
        {
            set => Call(() => IntelAdjNative.set_min_gfxclk_freq(_handle, checked((uint)Math.Round(value))));
        }

        public float MaxGfxClk_MHz
        {
            set => Call(() => IntelAdjNative.set_max_gfxclk_freq(_handle, checked((uint)Math.Round(value))));
        }

        public float StapmValue_W => WithLock(() => IntelAdjNative.get_stapm_value(_handle));
        public float FastValue_W => WithLock(() => IntelAdjNative.get_fast_value(_handle));
        public float SlowValue_W => WithLock(() => IntelAdjNative.get_slow_value(_handle));
        public float GfxVolt_V => WithLock(() => IntelAdjNative.get_gfx_volt(_handle));
        public float SocketPower_W => WithLock(() => IntelAdjNative.get_socket_power(_handle));

        #endregion

        #region Helper
        private void Call(Func<int> native)
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                int rc = native();
                IntelAdjException.ThrowIfError(rc);
            }
        }

        private T WithLock<T>(Func<T> getter)
        {
            lock (_lock)
            {
                EnsureNotDisposed();
                return getter();
            }
        }
        #endregion
    }
}
