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
    /// A multi-value convert that converts image thumbnail information into a scaled BitmapImage object.
    /// </summary>
    class ImageThumbnailConverter : IMultiValueConverter
    {
        /// <summary>
        /// Converts image thumbnail information into a scaled BitmapImage object. The image information
        /// consists of the filepath, rotation, and height data. The image is loaded at the size specified
        /// by the height value.
        /// </summary>
        /// <param name="values">
        /// Array of image information:
        /// - string : filepath
        /// - System.Windows.Media.Imaging.Rotation : rotation
        /// - int : thumbnail height
        /// </param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>A BitmapImage object for rendering the image.</returns>

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Get image path
            string path = values[0] as string;
            if (path == null)
                return null;
            
            // Initialize image
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path);

            // Apply rotation data if passed
            Rotation r = Rotation.Rotate0;
            if (values[1] is Rotation)
                r = (Rotation)values[1];
            image.Rotation = r;

            // If a thumbnail height has been specified, load the image at that size.
            // This saves memory when loading a bunch of thumbnails.
            if (values[2] != System.Windows.DependencyProperty.UnsetValue) {
                int decodeHeight = System.Convert.ToInt32(values[2]);
                image.DecodePixelHeight = decodeHeight;
            }

            image.EndInit();
            return image;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}