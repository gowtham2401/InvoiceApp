using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace InvoiceApp.Helpers
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = value is bool bv && bv;
            if (Invert) b = !b;
            return b ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class IdToFormTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int id && id > 0 ? $"Edit Customer #{id}" : "New Customer";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class ZeroToAutoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is int id && id > 0 ? id.ToString() : "(auto-assigned)";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class StatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value as string;
            string color;

            switch (status)
            {
                case "Paid":
                    color = "#27AE60";
                    break;

                case "Pending":
                    color = "#F39C12";
                    break;

                case "Overdue":
                    color = "#E74C3C";
                    break;

                case "Draft":
                    color = "#7F8C8D";
                    break;

                default:
                    color = "#95A5A6";
                    break;
            }

            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is decimal d ? $"Rs. {d:N2}" : "Rs. 0.00";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}