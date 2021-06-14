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
    /// Converts a boolean value to a string which can be used in a control's Visibility property.
    /// </summary>
    class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool vis = (bool) value;

            if (vis)
                return "Visible";

            // Collapsed means that the control will not take up any space along with being invisible.
            return "Collapsed";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
