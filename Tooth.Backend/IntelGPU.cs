using System;
using System.Threading;
using System.Timers;
using SharpDX.Direct3D9;
using Tooth.IGCL;
using static Tooth.IGCL.IGCLBackend;
using Timer = System.Timers.Timer;

namespace Tooth.GraphicsProcessingUnit
{
    public class IntelGPU : IDisposable
    {
        protected object updateLock = new();
        public bool IsInitialized = false;
        private bool busyEventRaised = false;
        protected object functionLock = new();
        protected volatile bool halting = false;
        public event StatusChangedEvent StatusChanged;
        public delegate void StatusChangedEvent(bool status);

        public AdapterInformation adapterInformation;
        protected int deviceIdx = -1;
        protected int displayIdx = 0;

        private bool _disposed = false; // Prevent multiple disposals

        public enum Vsync_Mode : uint
        {
            APPLICATION_CHOICE = 0,
            VSYNC_OFF = 1,
            VSYNC_ON = 2,
            SMOOTH_SYNC = 3, // Blur distracting screen tears with a dithering filters.
            SPEED_SYNC = 4, // Speed up the latest frame, Low Latency, No tearing, no cap.
            CAPPED_FPS = 5, // Automatically enables V-Sync when the application’s render rate exceeds the display's refresh rate and disables VSync when the render rate falls below the display's refresh rate.
        }
        public void Dispose()
        {
            halting = true;
            IsInitialized = false;
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            halting = true;
            IsInitialized = false;
            if (_disposed) return;

            if (disposing)
            {
            }

            _disposed = true;
        }

        protected T Execute<T>(Func<T> func, T defaultValue)
        {
            if (!halting && IsInitialized)
            {
                lock (functionLock)
                {
                    try
                    {
                        // Reset flag
                        busyEventRaised = false;

                        // Execute function
                        T result = func();

                        // If the busy event was raised, signal that we're now free.
                        if (busyEventRaised)
                            StatusChanged?.Invoke(false);

                        return result;
                    }
                    catch { }
                }
            }

            return defaultValue;
        }

        #region events
        public event EnduranceGamingStateEventHandler EnduranceGamingState;
        public delegate void EnduranceGamingStateEventHandler(bool Supported, ctl_3d_endurance_gaming_control_t Control, ctl_3d_endurance_gaming_mode_t Mode);
        #endregion

        private bool prevEnduranceGamingSupport;
        private ctl_3d_endurance_gaming_control_t prevEGControl = new();
        private ctl_3d_endurance_gaming_mode_t prevEGMode = new();

        protected ctl_telemetry_data TelemetryData = new();

        public bool HasIntegerScalingSupport()
        {
            if (!IsInitialized)
                return false;

            return Execute(() => IGCLBackend.HasIntegerScalingSupport(deviceIdx, 0), false);
        }

        public bool HasGPUScalingSupport()
        {
            if (!IsInitialized)
                return false;

            return Execute(() => IGCLBackend.HasGPUScalingSupport(deviceIdx, 0), false);
        }

        public bool HasScalingModeSupport()
        {
            if (!IsInitialized)
                return false;

            return Execute(() => IGCLBackend.HasGPUScalingSupport(deviceIdx, 0), false);
        }

        public bool GetGPUScaling()
        {
            if (!IsInitialized)
                return false;

            return Execute(() => IGCLBackend.GetGPUScaling(deviceIdx, 0), false);
        }

        public bool GetImageSharpening()
        {
            if (!IsInitialized)
                return false;

            return Execute(() => IGCLBackend.GetImageSharpening(deviceIdx, 0), false);
        }

        public int GetImageSharpeningSharpness()
        {
            if (!IsInitialized)
                return 0;

            return Execute(() => IGCLBackend.GetImageSharpeningSharpness(deviceIdx, 0), 0);
        }

        public bool GetIntegerScaling()
        {
            if (!IsInitialized)
                return false;

            return IGCLBackend.GetIntegerScaling(deviceIdx);
        }

        // GPUScaling can't be disabled on Intel GPU ?
        public bool SetGPUScaling(bool enabled)
        {
            if (!IsInitialized)
                return false;

            return IGCLBackend.SetGPUScaling(deviceIdx, 0 , enabled);
        }

        public bool SetImageSharpening(bool enable)
        {
            if (!IsInitialized)
                return false;

            return Execute(() => IGCLBackend.SetImageSharpening(deviceIdx, 0, enable), false);
        }

        public bool SetImageSharpeningSharpness(int sharpness)
        {
            if (!IsInitialized)
                return false;

            return Execute(() => IGCLBackend.SetImageSharpeningSharpness(deviceIdx, 0, sharpness), false);
        }

        public bool SetScalingMode(int mode)
        {
            if (!IsInitialized)
                return false;

            return IGCLBackend.SetScalingMode(deviceIdx, 0, mode);
        }

        public bool SetIntegerScaling(bool enabled, byte type)
        {
            if (!IsInitialized)
                return false;

            return IGCLBackend.SetIntegerScaling(deviceIdx, enabled, type);
        }

        // helper to test whether enumValue is supported:
        bool IsSupported<T>(uint mask, T enumValue) where T : Enum
        {
            int idx = Convert.ToInt32(enumValue);
            return ((mask >> idx) & 1) != 0;
        }

        public bool HasEnduranceGaming(out bool autoSupported, out bool onSupported, out bool offSupported)
        {
            autoSupported = false;
            onSupported = false;
            offSupported = false;

            if (!IsInitialized)
                return false;

            ctl_endurance_gaming_caps_t caps = GetEnduranceGamingCapacities();
            ctl_3d_endurance_gaming_control_t supportedControls = (ctl_3d_endurance_gaming_control_t)caps.EGControlCaps.SupportedTypes;
            ctl_3d_endurance_gaming_mode_t supportedModes = (ctl_3d_endurance_gaming_mode_t)caps.EGModeCaps.SupportedTypes;

            offSupported = IsSupported((uint)supportedControls, ctl_3d_endurance_gaming_control_t.OFF);
            onSupported = IsSupported((uint)supportedControls, ctl_3d_endurance_gaming_control_t.ON);
            autoSupported = IsSupported((uint)supportedControls, ctl_3d_endurance_gaming_control_t.AUTO);

            return autoSupported || onSupported;
        }

        public ctl_endurance_gaming_caps_t GetEnduranceGamingCapacities()
        {
            if (!IsInitialized)
                return new();

            //ctl_endurance_gaming_caps_t caps = new ctl_endurance_gaming_caps_t();
            return Execute(() => IGCLBackend.GetEnduranceGamingCapacities(deviceIdx), new());
        }

        public bool SetEnduranceGaming(ctl_3d_endurance_gaming_control_t control, ctl_3d_endurance_gaming_mode_t mode)
        {
            if (!IsInitialized)
                return false;

            return IGCLBackend.SetEnduranceGaming(
                deviceIdx,
                control,
                mode);
        }

        public ctl_endurance_gaming_t GetEnduranceGaming()
        {
            if (!IsInitialized)
                return new();

            return IGCLBackend.GetEnduranceGaming(deviceIdx);
        }


        public bool SetFPSLimiter(bool isLimiterEnabled, int fpsLimitValue)
        {
            if (!IsInitialized)
                return false;

            return IGCLBackend.SetFPSLimit(
                deviceIdx,
                isLimiterEnabled,
                fpsLimitValue);
        }

        public ctl_fps_limiter_t GetFPSLimiter()
        {
            if (!IsInitialized)
                return new();

            return IGCLBackend.GetFPSLimit(deviceIdx);
        }

        public bool SetLowLatency(ctl_3d_low_latency_types_t setting)
        {
            if (!IsInitialized)
                return false;

            return IGCLBackend.SetLowLatency(
                deviceIdx,
                setting);
        }

        public ctl_3d_low_latency_types_t GetLowLatency()
        {
            if (!IsInitialized)
                return new();

            return IGCLBackend.GetLowLatency(deviceIdx);
        }


        public bool SetFrameSyncMode(Vsync_Mode mode)
        {
            if (!IsInitialized)
                return false;
            switch (mode)
            {
                case Vsync_Mode.APPLICATION_CHOICE:
                    return IGCLBackend.SetFrameSync(deviceIdx, ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_APPLICATION_DEFAULT);
                case Vsync_Mode.VSYNC_OFF:
                    return IGCLBackend.SetFrameSync(deviceIdx, ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_VSYNC_OFF);
                case Vsync_Mode.VSYNC_ON:
                    return IGCLBackend.SetFrameSync(deviceIdx, ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_VSYNC_ON);
                case Vsync_Mode.SMOOTH_SYNC:
                    return IGCLBackend.SetFrameSync(deviceIdx, ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_SMOOTH_SYNC);
                case Vsync_Mode.SPEED_SYNC:
                    return IGCLBackend.SetFrameSync(deviceIdx, ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_SPEED_FRAME);
                case Vsync_Mode.CAPPED_FPS:
                    return IGCLBackend.SetFrameSync(deviceIdx, ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_CAPPED_FPS);
                default:
                    return false;
            }
        }

        public Vsync_Mode GetFrameSyncMode()
        {
            if (!IsInitialized)
                return new();

            ctl_gaming_flip_mode_flag_t mode = IGCLBackend.GetFrameSync(deviceIdx);
            switch (mode)
            {
                case ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_APPLICATION_DEFAULT:
                    return Vsync_Mode.APPLICATION_CHOICE;
                case ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_VSYNC_OFF:
                    return Vsync_Mode.VSYNC_OFF;
                case ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_VSYNC_ON:
                    return Vsync_Mode.VSYNC_ON;
                case ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_SMOOTH_SYNC:
                    return Vsync_Mode.SMOOTH_SYNC;
                case ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_SPEED_FRAME:
                    return Vsync_Mode.SPEED_SYNC;
                case ctl_gaming_flip_mode_flag_t.CTL_GAMING_FLIP_MODE_FLAG_CAPPED_FPS:
                    return Vsync_Mode.CAPPED_FPS;
                default:
                    return Vsync_Mode.APPLICATION_CHOICE;
            }
        }

        private ctl_telemetry_data GetTelemetry()
        {
            if (!IsInitialized)
                return TelemetryData;

            return Execute(() =>
            {
                return IGCLBackend.GetTelemetry(deviceIdx);
            }, TelemetryData);
        }

        public bool HasClock()
        {
            return TelemetryData.GpuCurrentClockFrequencySupported;
        }

        public float GetClock()
        {
            return (float)TelemetryData.GpuCurrentClockFrequencyValue;
        }

        public bool HasLoad()
        {
            return TelemetryData.GlobalActivitySupported;
        }

        public float GetLoad()
        {
            return (float)TelemetryData.GlobalActivityValue;
        }

        public bool HasPower()
        {
            return TelemetryData.GpuEnergySupported;
        }

        public float GetPower()
        {
            return (float)TelemetryData.GpuEnergyValue;
        }

        public bool HasTemperature()
        {
            return TelemetryData.GpuCurrentTemperatureSupported;
        }

        public float GetTemperature()
        {
            return (float)TelemetryData.GpuCurrentTemperatureValue;
        }

        public IntelGPU()
        {
            // Try to initialized IGCL
            bool IsLoaded_IGCL = IGCLBackend.Initialize();
            if (IsLoaded_IGCL)
                Console.WriteLine("IntelGPU() was successfully initialized", "IGCL");
            else
                Console.WriteLine("Failed to initialize {0}", "IGCL");

            // Get all adapters
            AdapterCollection adapters = new Direct3D().Adapters;

            // Todo: handle multiple adapters, This assumes single Handheld monitor setup.
            AdapterInformation primaryDisplay = null;

            foreach (AdapterInformation adapterInformation in adapters)
            {
                primaryDisplay = adapterInformation;
            }

            if (primaryDisplay == null)
                return;

            Console.WriteLine($"Primary Display Adapter : {primaryDisplay.Details.Description}");

            deviceIdx = IGCLBackend.GetDeviceIdx(primaryDisplay.Details.Description);
            if (deviceIdx == -1)
                return;

            Console.WriteLine($"Primary Display device idx is {deviceIdx}");

            IsInitialized = true;
        }

        protected void UpdateSettings()
        {
            if (Monitor.TryEnter(updateLock))
            {
                try
                {
                    ctl_endurance_gaming_t EnduranceGaming = new();
                    ctl_endurance_gaming_caps_t EnduranceGamingCaps = new();

                    bool EnduranceGamingOff = false;
                    bool EnduranceGamingOn = false;
                    bool EnduranceGamingAuto = false;

                    try
                    {
                        bool EnduranceGamingSupport = HasEnduranceGaming(out EnduranceGamingOff, out EnduranceGamingOn, out EnduranceGamingAuto);
                        if (EnduranceGamingSupport)
                        {
                            EnduranceGaming = GetEnduranceGaming();
                            EnduranceGamingCaps = GetEnduranceGamingCapacities();
                        }

                        // raise event
                        if (EnduranceGamingSupport != prevEnduranceGamingSupport || EnduranceGaming.EGControl != prevEGControl || EnduranceGaming.EGMode != prevEGMode)
                            EnduranceGamingState?.Invoke(EnduranceGamingSupport, EnduranceGaming.EGControl, EnduranceGaming.EGMode);

                        prevEnduranceGamingSupport = EnduranceGamingSupport;
                        prevEGControl = EnduranceGaming.EGControl;
                        prevEGMode = EnduranceGaming.EGMode;
                    }
                    catch { }
                }
                catch { }
                finally
                {
                    Monitor.Exit(updateLock);
                }
            }
        }
    }
}
