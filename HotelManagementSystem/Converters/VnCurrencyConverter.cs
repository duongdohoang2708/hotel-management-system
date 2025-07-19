using System;
using System.Globalization;
using System.Windows.Data;

namespace HotelManagementSystem.Converters
{
    public class VnCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            if (decimal.TryParse(value.ToString(), out decimal number))
            {
                // Định dạng: 1.234.567
                return number.ToString("N0", new CultureInfo("vi-VN"));
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0m;
            var str = value.ToString()?.Replace(".", "").Replace(",", "").Trim();
            if (decimal.TryParse(str, out decimal number))
            {
                return number;
            }
            return 0m;
        }
    }
} 