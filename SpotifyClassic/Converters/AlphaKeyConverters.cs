using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SpotifyClassic.Converters
{
    public class GroupToBackgroundBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection collection = value as ICollection;
            if (collection != null && collection.Count > 0)
            {
                // Devuelve tu recurso global personalizado
                return (SolidColorBrush)Application.Current.Resources["SpotifyAccentBrush"];
            }

            return (SolidColorBrush)Application.Current.Resources["PhoneChromeBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class GroupToForegroundBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection collection = value as ICollection;
            if (collection != null && collection.Count > 0)
            {
                return new SolidColorBrush(Colors.White);
            }

            return (SolidColorBrush)Application.Current.Resources["PhoneDisabledBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}