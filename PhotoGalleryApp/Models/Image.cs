using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// A Media object that refers to an image file.
    /// </summary>
    public class Image : Media
    {
        #region Constructors

        /// <summary>
        /// Creates an Image object with the given filepath.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        public Image(string path) : base(path) { }

        /// <summary>
        /// Creates an Image object with the given path, with the given list of tags.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        /// <param name="tags">A list of tags that the image should have.</param>
        public Image(string path, ObservableCollection<string> tags) : base(path, tags) { }

        /**
         * Needed by XmlSerializer
         */
        protected Image() : base() { }

        #endregion Constructors


        /**
         * Loads image metadata, including size and rotation.
         */
        protected override void LoadMetadata()
        {
            if (Filepath == null || !File.Exists(Filepath))
                return;


            // Load image metadata
            FileStream fs = new FileStream(Filepath, FileMode.Open, FileAccess.Read);
            BitmapFrame frame = BitmapFrame.Create(fs, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            BitmapMetadata meta = frame.Metadata as BitmapMetadata;


            // Image size
            _width = frame.PixelWidth;
            _height = frame.PixelHeight;


            // Process any rotation metadata
            Rotation = Rotation.Rotate0;
            if (meta != null && meta.ContainsQuery("System.Photo.Orientation"))
            {
                ushort rotAmount = (ushort)meta.GetQuery("System.Photo.Orientation");
                switch (rotAmount)
                {
                    case 6:
                        Rotation = Rotation.Rotate90;
                        break;
                    case 3:
                        Rotation = Rotation.Rotate180;
                        break;
                    case 8:
                        Rotation = Rotation.Rotate270;
                        break;
                }
            }

            fs.Close();
        }


        /*
         * Returns the media file type (image, video, etc.). This is used to determine
         * which Media subclass a Media instance belongs to.
         */
        protected override MediaFileType GetMediaType()
        {
            return MediaFileType.Image;
        }
    }
}
