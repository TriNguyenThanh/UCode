using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace UCode.Desktop.Helpers
{
    public class DaysLeftToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int daysLeft)
            {
                // Same logic as Web: red if <= 2 days, orange if <= 7 days, else blue
                if (daysLeft <= 2)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                }
                if (daysLeft <= 7)
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9800"));
                }
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2196F3"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

