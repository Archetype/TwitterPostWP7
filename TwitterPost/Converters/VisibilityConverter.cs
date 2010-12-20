using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections;

namespace TwitterPost
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                if (parameter is string && (string)parameter == "!")
                {
                    return ((bool)value) ? Visibility.Collapsed : Visibility.Visible;
                }
                return ((bool)value) ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (value is double)
            {
                if (parameter is string && (string)parameter == "!")
                {
                    return ((double)value) != 0 ? Visibility.Collapsed : Visibility.Visible;
                }
                return ((double)value) != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (value is IList)
            {
                if (parameter is string && (string)parameter == "!")
                {
                    return ((IList)value).Count != 0 ? Visibility.Collapsed : Visibility.Visible;
                }
                return ((IList)value).Count != 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                if (parameter is string && (string)parameter == "!")
                {
                    return value == null ? Visibility.Visible : Visibility.Collapsed;
                }
                return value == null ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
