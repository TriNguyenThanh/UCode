using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UCode.Desktop.Helpers
{
    public class BooleanToYesNoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string trueText = "Có";
            string falseText = "Không";

            if (parameter is string paramString && paramString.Contains("|"))
            {
                var parts = paramString.Split('|');
                if (parts.Length >= 2)
                {
                    trueText = parts[0];
                    falseText = parts[1];
                }
            }

            if (value is bool boolValue)
            {
                return boolValue ? trueText : falseText;
            }
            return falseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValidStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid)
            {
                return isValid ? "Hợp lệ" : "Không hợp lệ";
            }
            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
