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

        public double Resolution
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

        public void SetResolutionVar(double value)
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
        public bool fpsLimitEnabled = false;
        public double fpsLimitValue = 90;
        public double fpsMin = 30;
        public double fpsMax = 120;
        public double boostMode = 2; // 0: off, 1: enabled, 2: agressive
        public double enduranceGaming = 0; // 0: off, 1: Performance, 2: Balanced, 3: Battery
        public double resolution = 0; // 0: 1920x1200, 1: 1680 x 1050, 2: 1440 x 900, 3: 1280 x 800
        public bool autoStart = false;
        public bool isConnected = false;
        public double lowlatency = 0; // 0: off, 1: ON, 2: ON+BOOST
        public double framesync = 0; 

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
