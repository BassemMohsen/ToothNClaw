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
	internal class ColorRemasterModelWrapper : INotifyPropertyChanged, IDisposable
	{
		private ColorRemasterModel _base;
		private CoreDispatcher _dispatcher;


		public ColorRemasterModelWrapper(ColorRemasterModel baseModel, CoreDispatcher dispatcher)
		{
			_base = baseModel;
			_dispatcher = dispatcher;
		}

        public double HueValue
        {
            get { lock (_base) { return _base.hueValue; } }
            set
            {
                lock (_base)
                {
                    value = Math.Min(Math.Max(value, _base.hueMin), _base.hueMax);
                    if (_base.hueValue != value)
                    {
                        _base.hueValue = value;
                        _base.Notify("HueValue");
                        Backend.Instance.Send($"set-Hue-Value {value}");
                    }
                }
            }
        }

        public double HueMax
        {
            get { lock (_base) { return _base.hueMax; } }
            set
            {
                lock (_base)
                {
                    if (_base.hueMax != value)
                    {
                        _base.hueMax = value;

                        _base.Notify("HueMax");
                    }
                }
            }
        }
        public double HueMin
        {
            get { lock (_base) { return _base.hueMin; } }
            set
            {
                lock (_base)
                {
                    if (_base.hueMin != value)
                    {
                        _base.hueMin = value;
                        _base.Notify("HueMin");
                    }
                }
            }
        }

        public void SetHueValueVar(double value)
		{
			lock (_base)
			{
				if (_base.hueValue != value)
				{
					_base.hueValue = value;
					_base.Notify("HueValue");
                    Backend.Instance.Send($"set-Hue-Value {value}");
                }
			}
		}


        public double SaturationValue
        {
            get { lock (_base) { return _base.saturationValue; } }
            set
            {
                lock (_base)
                {
                    value = Math.Min(Math.Max(value, _base.saturationMin), _base.saturationMax);
                    if (_base.saturationValue != value)
                    {
                        _base.saturationValue = value;
                        _base.Notify("SaturationValue");
                        Backend.Instance.Send($"set-Saturation-Value {value}");
                    }
                }
            }
        }

        public double SaturationMax
        {
            get { lock (_base) { return _base.saturationMax; } }
            set
            {
                lock (_base)
                {
                    if (_base.saturationMax != value)
                    {
                        _base.saturationMax = value;

                        _base.Notify("SaturationMax");
                    }
                }
            }
        }
        public double SaturationMin
        {
            get { lock (_base) { return _base.saturationMin; } }
            set
            {
                lock (_base)
                {
                    if (_base.saturationMin != value)
                    {
                        _base.saturationMin = value;
                        _base.Notify("SaturationMin");
                    }
                }
            }
        }

        public void SetSaturationValueVar(double value)
        {
            lock (_base)
            {
                if (_base.saturationValue != value)
                {
                    _base.saturationValue = value;
                    _base.Notify("SaturationValue");
                    Backend.Instance.Send($"set-Saturation-Value {value}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public double BrightnessValue
        {
            get { lock (_base) { return _base.brightnessValue; } }
            set
            {
                lock (_base)
                {
                    value = Math.Min(Math.Max(value, _base.brightnessMin), _base.brightnessMax);
                    if (_base.brightnessValue != value)
                    {
                        _base.brightnessValue = value;
                        _base.Notify("BrightnessValue");
                        Backend.Instance.Send($"set-Brightness-Value {value}");
                    }
                }
            }
        }

        public double BrightnessMax
        {
            get { lock (_base) { return _base.brightnessMax; } }
            set
            {
                lock (_base)
                {
                    if (_base.brightnessMax != value)
                    {
                        _base.brightnessMax = value;

                        _base.Notify("BrightnessMax");
                    }
                }
            }
        }
        public double BrightnessMin
        {
            get { lock (_base) { return _base.brightnessMin; } }
            set
            {
                lock (_base)
                {
                    if (_base.brightnessMin != value)
                    {
                        _base.brightnessMin = value;
                        _base.Notify("BrightnessMin");
                    }
                }
            }
        }

        public void SetBrightnessValueVar(double value)
        {
            lock (_base)
            {
                if (_base.brightnessValue != value)
                {
                    _base.brightnessValue = value;
                    _base.Notify("BrightnessValue");
                    Backend.Instance.Send($"set-Brightness-Value {value}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// 
        public double ContrastValue
        {
            get { lock (_base) { return _base.contrastValue; } }
            set
            {
                lock (_base)
                {
                    value = Math.Min(Math.Max(value, _base.contrastMin), _base.contrastMax);
                    if (_base.contrastValue != value)
                    {
                        _base.contrastValue = value;
                        _base.Notify("ContrastValue");
                        Backend.Instance.Send($"set-Contrast-Value {value}");
                    }
                }
            }
        }

        public double ContrastMax
        {
            get { lock (_base) { return _base.contrastMax; } }
            set
            {
                lock (_base)
                {
                    if (_base.contrastMax != value)
                    {
                        _base.contrastMax = value;

                        _base.Notify("ContrastMax");
                    }
                }
            }
        }
        public double ContrastMin
        {
            get { lock (_base) { return _base.contrastMin; } }
            set
            {
                lock (_base)
                {
                    if (_base.contrastMin != value)
                    {
                        _base.contrastMin = value;
                        _base.Notify("ContrastMin");
                    }
                }
            }
        }

        public void SetContrastValueVar(double value)
        {
            lock (_base)
            {
                if (_base.contrastValue != value)
                {
                    _base.contrastValue = value;
                    _base.Notify("ContrastValue");
                    Backend.Instance.Send($"set-Contrast-Value {value}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public double SharpnessValue
        {
            get { lock (_base) { return _base.sharpnessValue; } }
            set
            {
                lock (_base)
                {
                    value = Math.Min(Math.Max(value, _base.sharpnessMin), _base.sharpnessMax);
                    if (_base.sharpnessValue != value)
                    {
                        _base.sharpnessValue = value;
                        _base.Notify("SharpnessValue");
                        Backend.Instance.Send($"set-Sharpness-Value {value}");
                    }
                }
            }
        }

        public double SharpnessMax
        {
            get { lock (_base) { return _base.sharpnessMax; } }
            set
            {
                lock (_base)
                {
                    if (_base.sharpnessMax != value)
                    {
                        _base.sharpnessMax = value;

                        _base.Notify("SharpnessMax");
                    }
                }
            }
        }
        public double SharpnessMin
        {
            get { lock (_base) { return _base.sharpnessMin; } }
            set
            {
                lock (_base)
                {
                    if (_base.sharpnessMin != value)
                    {
                        _base.sharpnessMin = value;
                        _base.Notify("SharpnessMin");
                    }
                }
            }
        }

        public void SetSharpnessValueVar(double value)
        {
            lock (_base)
            {
                if (_base.sharpnessValue != value)
                {
                    _base.sharpnessValue = value;
                    _base.Notify("SharpnessValue");
                    Backend.Instance.Send($"set-Sharpness-Value {value}");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>

        public double GammaValue
        {
            get { lock (_base) { return _base.gammaValue; } }
            set
            {
                lock (_base)
                {
                    value = Math.Min(Math.Max(value, _base.gammaMin), _base.gammaMax);
                    if (_base.gammaValue != value)
                    {
                        _base.gammaValue = value;
                        _base.Notify("GammaValue");
                        Backend.Instance.Send($"set-Gamma-Value {value}");
                    }
                }
            }
        }

        public double GammaMax
        {
            get { lock (_base) { return _base.gammaMax; } }
            set
            {
                lock (_base)
                {
                    if (_base.gammaMax != value)
                    {
                        _base.gammaMax = value;

                        _base.Notify("GammaMax");
                    }
                }
            }
        }
        public double GammaMin
        {
            get { lock (_base) { return _base.gammaMin; } }
            set
            {
                lock (_base)
                {
                    if (_base.gammaMin != value)
                    {
                        _base.gammaMin = value;
                        _base.Notify("GammaMin");
                    }
                }
            }
        }

        public void SetGammaValueVar(double value)
        {
            lock (_base)
            {
                if (_base.gammaValue != value)
                {
                    _base.gammaValue = value;
                    _base.Notify("GammaValue");
                    Backend.Instance.Send($"set-Gamma-Value {value}");
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

	class ColorRemasterModel
    {
        public double hueMin = -180;
        public double hueMax = 180;
        public double hueValue = 0;

        public double saturationMin = 0;
        public double saturationMax = 100;
        public double saturationValue = 50;

        public double brightnessMin = 0;
        public double brightnessMax = 100;
        public double brightnessValue = 50;

        public double contrastMin = 0;
        public double contrastMax = 100;
        public double contrastValue = 50;

        public double sharpnessMin = 0;
        public double sharpnessMax = 100;
        public double sharpnessValue = 0;

        public double gammaMin = 0.3;
        public double gammaMax = 2.8;
        public double gammaValue = 1.0;

        private List<ColorRemasterModelWrapper> _wrappers = new List<ColorRemasterModelWrapper>();

		public ColorRemasterModelWrapper GetWrapper(CoreDispatcher dispatcher)
		{
			var wrapper = new ColorRemasterModelWrapper(this, dispatcher);
			_wrappers.Add(wrapper);
			return wrapper;
		}

		public void Notify(string propertyName)
		{
			foreach (var wrapper in _wrappers)
				_ = wrapper.Notify(propertyName);
		}

		public void Remove(ColorRemasterModelWrapper wraper)
		{
			_wrappers.Remove(wraper);
		}
	}
}
