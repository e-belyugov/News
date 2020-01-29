using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace News.Forms.UI.Helpers
{
    /// <summary>
    /// Bool to grid length converter
    /// </summary>
    public class BoolToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!(value is bool)) return 0;

            bool boolValue = (bool) value;

            if (boolValue) return GridLength.Star;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
