using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace IntelAPUSettings
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

		public double Fps
        {
			get { lock (_base) { return _base.fps; } }
			set
			{
                lock (_base)
                {
					value = Math.Min(Math.Max(value, _base.fpsMin), _base.fpsMax);
					if (_base.fps != value)
					{
						_base.fps = value;
						_base.Notify("Fps");
						Backend.Instance.Send($"set-fps {value}");
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
        public void SetFpsVar(double value)
		{
			lock (_base)
			{
				if (_base.fps != value)
				{
					_base.fps = value;
					_base.Notify("Fps");
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
						if (_base.fps > value)
                            SetFpsVar(value);
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
						if (_base.fps < value)
							SetFpsVar(value);
						_base.Notify("FpsMin");
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
        public double fps = 90;
        public double fpsMin = 30;
        public double fpsMax = 120;
        public double boostMode = 2; // 0: off, 1: enabled, 2: agressive
        public bool isConnected = false;

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
