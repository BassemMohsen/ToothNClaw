using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace Tooth
{
    public struct Resolution
    {
        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Frequency { get; set; }
        public bool IsNative { get; set; }

        public string DisplayText
        {
            get
            {
                string aspect = GetAspectRatio(Width, Height);
                string nativeLabel = (IsNative == true) ? " (Native)" : "";
                return $"{Width} x {Height} ({aspect}){nativeLabel}";
            }
        }
        // Helper method to calculate common aspect ratios
        private static string GetAspectRatio(int width, int height)
        {
            double ratio = (double)width / height;

            // Match to common display ratios with a small tolerance
            if (Math.Abs(ratio - 16.0 / 9) < 0.01) return "16:9";
            if (Math.Abs(ratio - 16.0 / 10) < 0.01) return "16:10";
            if (Math.Abs(ratio - 4.0 / 3) < 0.01) return "4:3";
            if (Math.Abs(ratio - 5.0 / 4) < 0.01) return "5:4";
            if (Math.Abs(ratio - 21.0 / 9) < 0.01) return "21:9";

            // fallback to reduced fraction if unknown
            int gcd = GCD(width, height);
            return $"{width / gcd}:{height / gcd}";
        }

        // Euclidean algorithm to find greatest common divisor
        private static int GCD(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }
    }

    internal class MainPageModelWrapper : INotifyPropertyChanged, IDisposable
	{
		private MainPageModel _base;
		private CoreDispatcher _dispatcher;



        public MainPageModelWrapper(MainPageModel baseModel, CoreDispatcher dispatcher)
		{
			_base = baseModel;
			_dispatcher = dispatcher;
		}

        public bool FpsLimitEnabled
        {
            get { lock (_base) { return _base.fpsLimitEnabled; } }
            set
            {
                lock (_base)
                {
                    if (_base.fpsLimitEnabled != value)
                    {
                        _base.fpsLimitEnabled = value;
                        _base.Notify("FpsLimitEnabled");
                        Backend.Instance.Send($"set-Fps-limiter {Convert.ToInt32(value)} {_base.fpsLimitValue}");
                    }
                }
            }
        }

        public List<Resolution> Resolutions
        {
            get { lock (_base) { return _base.resolutions; } }
            set
            {
                lock (_base)
                {
                    if (_base.resolutions != value)
                    {
                        _base.resolutions = value;
                        _base.Notify("Resolutions");
                    }
                }
            }
        }


        public double FpsLimitValue
        {
			get { lock (_base) { return _base.fpsLimitValue; } }
			set
			{
                lock (_base)
                {
					value = Math.Min(Math.Max(value, _base.fpsMin), _base.fpsMax);
					if (_base.fpsLimitValue != value)
					{
						_base.fpsLimitValue = value;
						_base.Notify("FpsLimitValue");
						Backend.Instance.Send($"set-Fps-limiter {Convert.ToInt32(_base.fpsLimitEnabled)} {value}");
					}
				}
			}
		}

        public double BoostMode
        {
            get { lock (_base) { return _base.boostMode; } }
            set
            {
                lock (_base)
                {
                    if (_base.boostMode != value)
                    {
                        _base.boostMode = value;
                        _base.Notify("BoostMode");
                        Backend.Instance.Send($"set-boost {value}");
                    }
                }
            }
        }

        public int Resolution
        {
            get { lock (_base) { return _base.resolution; } }
            set
            {
                lock (_base)
                {
                    if (_base.resolution != value)
                    {
                        _base.resolution = value;
                        _base.Notify("Resolution");
                        Backend.Instance.Send($"set-resolution {value}");
                    }
                }
            }
        }

        public double EnduranceGaming
        {
            get { lock (_base) { return _base.enduranceGaming; } }
            set
            {
                lock (_base)
                {
                    if (_base.enduranceGaming != value)
                    {
                        _base.enduranceGaming = value;
                        _base.Notify("EnduranceGaming");
                        Backend.Instance.Send($"set-EnduranceGaming {value}");
                    }
                }
            }
        }

        public double LowLatency
        {
            get { lock (_base) { return _base.lowlatency; } }
            set
            {
                lock (_base)
                {
                    if (_base.lowlatency != value)
                    {
                        _base.lowlatency = value;
                        _base.Notify("LowLatency");
                        Backend.Instance.Send($"set-Low-Latency-Mode {value}");
                    }
                }
            }
        }

        public double FrameSync
        {
            get { lock (_base) { return _base.framesync; } }
            set
            {
                lock (_base)
                {
                    if (_base.framesync != value)
                    {
                        _base.framesync = value;
                        _base.Notify("FrameSync");
                        Backend.Instance.Send($"set-Frame-Sync-Mode {value}");
                    }
                }
            }
        }

        public void SetFpsLimiterValueVar(double value)
		{
			lock (_base)
			{
				if (_base.fpsLimitValue != value)
				{
					_base.fpsLimitValue = value;
					_base.Notify("FpsLimitValue");
				}
			}
		}

        public void SetFpsLimiterEnabledVar(bool value)
        {
            lock (_base)
            {
                if (_base.fpsLimitEnabled != value)
                {
                    _base.fpsLimitEnabled = value;
                    _base.Notify("FpsLimitEnabled");
                }
            }
        }

        public void SetBoostVar(double value)
        {
            lock (_base)
            {
                if (_base.boostMode != value)
                {
                    _base.boostMode = value;
                    Backend.Instance.Send($"set-boost {value}");
                    _base.Notify("BoostMode");
                }
            }
        }

        public void SetResolutionVar(int value)
        {
            lock (_base)
            {
                if (_base.resolution != value)
                {
                    _base.resolution = value;
                    Backend.Instance.Send($"set-resolution {value}");
                    _base.Notify("Resolution");
                }
            }
        }

        public void SetEnduranceGamingVar(double value)
        {
            lock (_base)
            {
                if (_base.enduranceGaming != value)
                {
                    _base.enduranceGaming = value;
                    Backend.Instance.Send($"set-EnduranceGaming {value}");
                    _base.Notify("EnduranceGaming");
                }
            }
        }

        public void SetLowLatencyVar(double value)
        {
            lock (_base)
            {
                if (_base.lowlatency != value)
                {
                    _base.lowlatency = value;
                    Backend.Instance.Send($"set-Low-Latency-Mode {value}");
                    _base.Notify("LowLatency");
                }
            }
        }

        public void SetFrameSyncVar(double value)
        {
            lock (_base)
            {
                if (_base.framesync != value)
                {
                    _base.framesync = value;
                    Backend.Instance.Send($"set-Frame-Sync-Mode {value}");
                    _base.Notify("FrameSync");
                }
            }
        }

        public double FpsMax
		{
			get { lock (_base) { return _base.fpsMax; } }
			set
			{
				lock (_base)
				{
					if (_base.fpsMax != value)
					{
						_base.fpsMax = value;

						_base.Notify("FpsMax");
					}
				}
			}
		}
		public double FpsMin
		{
			get { lock (_base) { return _base.fpsMin; } }
			set
			{
				lock (_base)
				{
					if (_base.fpsMin != value)
					{
						_base.fpsMin = value;
						_base.Notify("FpsMin");
					}
				}
			}
		}
        public bool AutoStart
        {
            get { lock (_base) { return _base.autoStart; } }
            set
            {
                lock (_base)
                {
                    if (_base.autoStart != value)
                    {
                        _base.autoStart = value;
                        _base.Notify("AutoStart");
                        Backend.Instance.Send($"autostart {value}");
                    }
                }
            }
        }
        public void SetAutoStartVar(bool value)
        {
            lock (_base)
            {
                if (_base.autoStart != value)
                {
                    _base.autoStart = value;
                    _base.Notify("AutoStart");
                }
            }
        }

        public int DeviceScaling
        {
            get { lock (_base) { return _base.deviceScaling; } }
            set
            {
                lock (_base)
                {
                    if (_base.deviceScaling != value)
                    {
                        _base.deviceScaling = value;

                        if (_base.deviceScaling == 0)
                        {
                            // Display Scaling
                            Backend.Instance.Send($"set-Scaling 0");
                        }
                        else if (_base.deviceScaling == 1)
                        {
                            if (_base.gpuScalingMode == 0)
                            {
                                // GPU Scaling - Maintain Aspect ratio
                                Backend.Instance.Send($"set-Scaling 1");
                            }
                            else if (_base.gpuScalingMode == 1)
                            {
                                // GPU Scaling - Stretch
                                Backend.Instance.Send($"set-Scaling 2");
                            }
                            else if (_base.gpuScalingMode == 2)
                            {
                                // GPU Scaling - Center
                                Backend.Instance.Send($"set-Scaling 3");
                            }
                            else
                            {
                                // Do nothing, wrong value for GPU Scaling
                            }
                        }
                        else if (_base.deviceScaling == 2)
                        {
                            if (_base.retroScalingMode == 0)
                            {
                                // Retro Scaling - Integer Scaling
                                Backend.Instance.Send($"set-Scaling 4");
                            }
                            else if (_base.retroScalingMode == 1)
                            {
                                // Retro Scaling - Nearest neighbour
                                Backend.Instance.Send($"set-Scaling 5");
                            }
                            else
                            {
                                // Do nothing, wrong value for Retro Scaling
                            }
                        }
                        else
                        {
                            // Do nothing, wrong value for Device Scaling
                        }
                        _base.Notify("DeviceScaling");
                    }
                }
            }
        }

        public void SetDeviceScalingVar(int value)
        {
            lock (_base)
            {
                if (_base.deviceScaling != value)
                {
                    _base.deviceScaling = value;

                    if (_base.deviceScaling == 0)
                    {
                        // Display Scaling
                        Backend.Instance.Send($"set-Scaling 0");
                    }
                    else if (_base.deviceScaling == 1)
                    {
                        if (_base.gpuScalingMode == 0)
                        {
                            // GPU Scaling - Maintain Aspect ratio
                            Backend.Instance.Send($"set-Scaling 1");
                        }
                        else if (_base.gpuScalingMode == 1)
                        {
                            // GPU Scaling - Stretch
                            Backend.Instance.Send($"set-Scaling 2");
                        }
                        else if (_base.gpuScalingMode == 2)
                        {
                            // GPU Scaling - Center
                            Backend.Instance.Send($"set-Scaling 3");
                        }
                        else
                        {
                            // Do nothing, wrong value for GPU Scaling
                        }
                    }
                    else if (_base.deviceScaling == 2)
                    {
                        if (_base.retroScalingMode == 0)
                        {
                            // Retro Scaling - Integer Scaling
                            Backend.Instance.Send($"set-Scaling 4");
                        }
                        else if (_base.retroScalingMode == 1)
                        {
                            // Retro Scaling - Nearest neighbour
                            Backend.Instance.Send($"set-Scaling 5");
                        }
                        else
                        {
                            // Do nothing, wrong value for Retro Scaling
                        }
                    }
                    else
                    {
                        // Do nothing, wrong value for Device Scaling
                    }
                    _base.Notify("DeviceScaling");
                }
            }
        }

        public int GpuScalingMode
        {
            get { lock (_base) { return _base.gpuScalingMode; } }
            set
            {
                lock (_base)
                {
                    if (_base.gpuScalingMode != value)
                    {
                        _base.gpuScalingMode = value;

                        if (_base.deviceScaling == 1)
                        {
                            if (_base.gpuScalingMode == 0)
                            {
                                // GPU Scaling - Maintain Aspect ratio
                                Backend.Instance.Send($"set-Scaling 1");
                            }
                            else if (_base.gpuScalingMode == 1)
                            {
                                // GPU Scaling - Stretch
                                Backend.Instance.Send($"set-Scaling 2");
                            }
                            else if (_base.gpuScalingMode == 2)
                            {
                                // GPU Scaling - Center
                                Backend.Instance.Send($"set-Scaling 3");
                            }
                            else
                            {
                                // Do nothing, wrong value for GPU Scaling
                            }
                        }

                        _base.Notify("GpuScalingMode");

                    }
                }
            }
        }

        public void SetGpuScalingModeVar(int value)
        {
            lock (_base)
            {
                if (_base.gpuScalingMode != value)
                {
                    _base.gpuScalingMode = value;

                    if (_base.deviceScaling == 1)
                    {
                        if (_base.gpuScalingMode == 0)
                        {
                            // GPU Scaling - Maintain Aspect ratio
                            Backend.Instance.Send($"set-Scaling 1");
                        }
                        else if (_base.gpuScalingMode == 1)
                        {
                            // GPU Scaling - Stretch
                            Backend.Instance.Send($"set-Scaling 2");
                        }
                        else if (_base.gpuScalingMode == 2)
                        {
                            // GPU Scaling - Center
                            Backend.Instance.Send($"set-Scaling 3");
                        }
                        else
                        {
                            // Do nothing, wrong value for GPU Scaling
                        }
                    }

                    _base.Notify("GpuScalingMode");

                }
            }
        }

        public int RetroScalingMode
        {
            get { lock (_base) { return _base.retroScalingMode; } }
            set
            {
                lock (_base)
                {
                    if (_base.retroScalingMode != value)
                    {
                        _base.retroScalingMode = value;

                        if (_base.deviceScaling == 2) // retro scaling
                        {
                            if (_base.retroScalingMode == 0)
                            {
                                // Retro Scaling - Integer Scaling
                                Backend.Instance.Send($"set-Scaling 4");
                            }
                            else if (_base.retroScalingMode == 1)
                            {
                                // GPU Scaling - Nearest Neighbour
                                Backend.Instance.Send($"set-Scaling 5");
                            }
                            else
                            {
                                // Do nothing, wrong value for Retro Scaling
                            }
                        }

                        _base.Notify("RetroScalingMode");
                    }
                }
            }
        }

        public void SetRetroScalingModeVar(int value)
        {
            lock (_base)
            {
                if (_base.retroScalingMode != value)
                {
                    _base.retroScalingMode = value;
                    if (_base.retroScalingMode < 2)
                    {
                        if (_base.deviceScaling == 2)
                        {
                            // Retro Scaling
                            Backend.Instance.Send($"set-Retro-Scaling {value}");
                        }
                    }
                    _base.Notify("RetroScalingMode");
                }
            }
        }

        public void SuspendActiveGame()
        {
            
            Backend.Instance.Send("suspend-active-game");
        }

        public void ResumeActiveGame()
        {

            Backend.Instance.Send("resume-active-game");
        }

        public bool AutoSuspendEnabled
        {
            get { lock (_base) { return _base.autoSuspendEnabled; } }
            set
            {
                lock (_base)
                {
                    if (_base.autoSuspendEnabled != value)
                    {
                        _base.autoSuspendEnabled = value;
                        _base.Notify("AutoSuspendEnabled");
                        Backend.Instance.Send($"set-auto-suspend {Convert.ToInt32(value)}");
                    }
                }
            }
        }

        public bool GoBackToSleepEnabled
        {
            get { lock (_base) { return _base.goBackToSleepEnabled; } }
            set
            {
                lock (_base)
                {
                    if (_base.goBackToSleepEnabled != value)
                    {
                        _base.goBackToSleepEnabled = value;
                        _base.Notify("GoBackToSleepEnabled");
                        Backend.Instance.Send($"set-go-back-to-sleep {Convert.ToInt32(value)}");
                    }
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

		public async Task Notify(string propertyName)
		{
			await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				if (PropertyChanged != null)
				{
					this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
				}
			});
		}

		public void Dispose()
		{
			_base.Remove(this);
		}
	}

	class MainPageModel
    {
        public List<Resolution> resolutions;
        public bool fpsLimitEnabled = false;
        public double fpsLimitValue = 90;
        public double fpsMin = 30;
        public double fpsMax = 120;
        public double boostMode = 2; // 0: off, 1: enabled, 2: agressive
        public double enduranceGaming = 0; // 0: off, 1: Performance, 2: Balanced, 3: Battery
        public int resolution = 0;
        public bool autoStart = false;
        public bool isConnected = false;
        public double lowlatency = 0; // 0: off, 1: ON, 2: ON+BOOST
        public double framesync = 0; 
        public int deviceScaling = 0;  // 0: Display Scaling, 1: Gpu Scaling, 2: Retro Scaling
        public int gpuScalingMode = 0;  // 0: Maintain Aspect Ratio, 1: Stretch, 2: Center
        public int retroScalingMode = 0; // 0: Integer Scaling, 1: Nearest neighbour
        public bool autoSuspendEnabled = false;
        public bool goBackToSleepEnabled = false;

        private List<MainPageModelWrapper> _wrappers = new List<MainPageModelWrapper>();

		public MainPageModelWrapper GetWrapper(CoreDispatcher dispatcher)
		{
			var wrapper = new MainPageModelWrapper(this, dispatcher);
			_wrappers.Add(wrapper);
			return wrapper;
		}

		public void Notify(string propertyName)
		{
			foreach (var wrapper in _wrappers)
				_ = wrapper.Notify(propertyName);
		}

		public void Remove(MainPageModelWrapper wraper)
		{
			_wrappers.Remove(wraper);
		}
	}
}
