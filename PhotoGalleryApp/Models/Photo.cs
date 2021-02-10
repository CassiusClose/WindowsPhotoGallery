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
    /// Represents one image, stored somewhere on disk, associated with a list of tags.
    /// </summary>
    public class Photo
    {
        #region Constructors

        public Photo(string path, ObservableCollection<string> tags)
        {
            Path = path;
            //LoadImage();

            if (tags == null)
                Tags = new ObservableCollection<string>();
            else
                Tags = tags;
        }

        private Photo() : this(null, null) { }

        public Photo(string path) : this(path, null) { }

        #endregion Constructors



        #region Fields and Properties

        /// <summary>
        /// How much the image should be rotated.
        /// </summary>
        public Rotation Rotation { get; set; }


        private string _path;
        /// <summary>
        /// The filepath to the image.
        /// </summary>
        public string Path 
        {
            get { return _path; }
            set
            {
                _path = value;
                // Load the image's rotation data
                LoadImage();
            }
        }


        /// <summary>
        /// A collection of tags associated with the image, used for easier sorting & filtering of images.
        /// </summary>
        public ObservableCollection<string> Tags { get; set; }

        #endregion Fields and Properties



        #region Methods

        /// <summary>
        /// Loads any informaton about this class' photo needed to display it.
        /// If the image has orientation metadata, reads and saves that.
        /// </summary>
        private void LoadImage()
        {
            if (Path == null)
                return;

            // By default, don't rotate the image
            Rotation = Rotation.Rotate0;

            FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            BitmapFrame frame = BitmapFrame.Create(fs, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            BitmapMetadata meta = frame.Metadata as BitmapMetadata;
            // If there is orientation metadata, process it
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



        /// <summary>
        /// Returns a string representation of this photo.
        /// </summary>
        /// <returns>A string representation of this photo.</returns>
        public override string ToString()
        {
            return Path;
        }

        #endregion Methods
    }
}
