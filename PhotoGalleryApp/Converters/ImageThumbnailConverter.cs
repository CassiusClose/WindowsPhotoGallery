using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace PhotoGalleryApp.Converters
{
    /// <summary>
    /// A multi-value convert that converts image thumbnail information into a BitmapImage object.
    /// </summary>
    class ImageThumbnailConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts image thumbnail information into a BitmapImage object. The image information
        /// consists of the filepath and rotation data.
        /// </summary>
        /// <param name="values">
        /// Array of image information:
        /// - string : filepath
        /// - System.Windows.Media.Imaging.Rotation : rotation
        /// </param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>A BitmapImage object for rendering the image.</returns>

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            string path = values[0] as string;
            if (path == null)
                return null;

            Rotation r = Rotation.Rotate0;
            if (values[1] is Rotation)
                r = (Rotation)values[1];
            
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);
            image.Rotation = r;
            image.EndInit();
            return image;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}