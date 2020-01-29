using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace News.Forms.UI.Helpers
{
    /// <summary>
    /// Bool to grid height converter
    /// </summary>
    public class BoolToGridHeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return 0;

            bool boolValue = (bool)value;

            if (!boolValue) return GridLength.Auto;

            return 180;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
