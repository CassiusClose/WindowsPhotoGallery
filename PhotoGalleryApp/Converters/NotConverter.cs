using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PhotoGalleryApp.Converters
{
    /// <summary>
    /// Converter to invert a boolean value
    /// </summary>
    class NotConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                throw new ArgumentException("NotConverter expects bool value");

            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not bool)
                throw new ArgumentException("NotConverter expects bool value");

            return !((bool)value);
        }
    }
}
