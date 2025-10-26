using Microsoft.UI.Xaml;
using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Tooth
{
    public class DisplayToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is double d)
            {
                // Hide when Display (0) is selected
                return d == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
