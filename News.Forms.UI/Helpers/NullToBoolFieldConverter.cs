using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Globalization;

namespace News.Forms.UI.Helpers
{
    /// <summary>
    /// Null to bool converter
    /// </summary>
    public class NullValueBoolConverter : IValueConverter, IMarkupExtension
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is string)
            {
                if (string.IsNullOrEmpty(value as string))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {

                if (value == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
