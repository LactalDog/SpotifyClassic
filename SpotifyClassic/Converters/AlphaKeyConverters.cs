using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using SpotifyClassic.Data;
using SpotifyClassic.ViewModels;

namespace SpotifyClassic.Converters
{
    public class GroupToBackgroundBrushValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var group = value as AlphaKeyGroup<AlbumModel>;
            if (group != null && group.Count > 0)
            {
                return (SolidColorBrush)Application.Current.Resources["PhoneAccentBrush"];
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
            var group = value as AlphaKeyGroup<AlbumModel>;
            if (group != null && group.Count > 0)
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