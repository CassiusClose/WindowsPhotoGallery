using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PhotoGalleryApp.Converters
{
    /**
     * Converts PrecisionDateTime to DateTime and back for use with the
     * DatePicker control. Converting PrecisionDateTime to DateTime
     * means increasing the precision & generating new data. Converting
     * DateTime to PrecisionDateTime means doing the opposite.
     */
    public class PrecisionDateTimeToDateTimeConverter : IValueConverter
    {
        /**
         * value is the PrecisionDateTime to convert. This will almost always mean generating new
         * timestamp data to increase the precision of the timestamp. The parameter argument should
         * be a bool, whether to create this new timestamp data as if this DateTime will be used as
         * the beginning or the end of a time range. So if the PrecisionDateTime is of the Month
         * precision, a start timestamp would be at the first day of the month, at 00:00:00. An end
         * timestamp would be the last day of the month at 23:59:59.
         */
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;


            if (value is not PrecisionDateTime)
                throw new ArgumentNullException(nameof(value));

            if (parameter is not bool)
                throw new ArgumentNullException(nameof(parameter));


            return ((PrecisionDateTime)value).ToDateTime((bool)parameter);
        }

        /**
         * Because this is meant to be used with DatePicker, assume the
         * precision is always Day level. DatePickers can deal with time.
         */
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return null;


            if(value is not DateTime)
                throw new ArgumentNullException(nameof(value));

            if(parameter is not bool)
                throw new ArgumentNullException(nameof(parameter));

            return new PrecisionDateTime((DateTime)value, TimeRange.Day, (bool)parameter);
        }
    }
}
