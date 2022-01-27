using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PhotoGalleryApp.Converters
{
    class HeightARToWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Check that the binding values exist and are have values
            if (values.Length != 2)
                return 0.0;
            for(int i = 0; i < values.Length; i++)
            {
                if (values[i] == System.Windows.DependencyProperty.UnsetValue)
                    return 0.0;
            }

            // Convert values
            double height = (double)values[0];
            double ar = (double)values[1];

            // Calculate
            return height * ar;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
