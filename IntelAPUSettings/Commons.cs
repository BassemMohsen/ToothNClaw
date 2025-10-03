using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace IntelAPUSettings
{

    public class WattConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{value} W";
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AdjustSliderCommand : ICommand
    {
        public double Step { get; set; }

        public bool CanExecute(object parameter)
        {
            return parameter is Slider;
        }

        public void Execute(object parameter)
        {
            if (parameter is Slider slider)
            {
                double newValue = slider.Value + Step;

                if (newValue > slider.Maximum)
                    newValue = slider.Maximum;
                if (newValue < slider.Minimum)
                    newValue = slider.Minimum;

                slider.Value = newValue;
            }
        }

        public event EventHandler CanExecuteChanged;
    }
}
