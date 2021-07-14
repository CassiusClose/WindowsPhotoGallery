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
    /// Represents one piece of media, photo or image, associated with a list of tags.
    /// </summary>
    public class Media
    {
        #region Constructors

        /// <summary>
        /// Creates a media object with the given path, with the given list of tags.
        /// </summary>
        /// <param name="path">The filepath of the media object.</param>
        /// <param name="tags">A list of tags that the media object should have.</param>
        public Media(string path, ObservableCollection<string> tags)
        {
            Path = path;

            if (tags == null)
                Tags = new ObservableCollection<string>();
            else
                Tags = tags;
        }


        /// <summary>
        /// Creates a blank media object.
        /// </summary>
        private Media() : this(null, null) { }


        /// <summary>
        /// Creates a media object with the given filepath.
        /// </summary>
        /// <param name="path"></param>
        public Media(string path) : this(path, null) { }

        #endregion Constructors



        #region Fields and Properties


        private string _path;
        /// <summary>
        /// The filepath of the piece of media.
        /// </summary>
        public string Path 
        {
            get { return _path; }
            set
            {
                _path = value;

                if (value != null)
                {
                    // Extract the extension from the path
                    string[] strs = _path.Split('.');
                    _extension = strs[strs.Length - 1].ToLower();

                    // Load the media's metadata
                    if (IsVideo)
                        LoadVideo();
                    else
                        LoadImage();
                }
            }
        }


        private string _extension;
        /// <summary>
        /// The media file's extension
        /// </summary>
        public string Extension { get { return _extension; } }


        /// <summary>
        /// Whether the media object is a video (true) or an image (false)
        /// </summary>
        public bool IsVideo
        {
            get
            {
                if(Extension == "mp4")
                    return true;
                return false;
            }
        }


        /// <summary>
        /// How much the image should be rotated.
        /// </summary>
        public Rotation Rotation { get; set; }


        private int _width;
        /// <summary>
        /// The width of the image
        /// </summary>
        public int Width 
        {
            get { return _width; }
        }

        private int _height;
        /// <summary>
        /// The height of the image
        /// </summary>
        public int Height 
        {
            get { return _height; }
        }

        /// <summary>
        /// The aspect ratio (width/height) of the image
        /// </summary>
        public float AspectRatio
        {
            get { return Width / (float) Height;  }
        }



        /// <summary>
        /// A collection of tags associated with the image, used for easier sorting & filtering of images.
        /// </summary>
        public ObservableCollection<string> Tags { get; set; }

        #endregion Fields and Properties



        #region Methods

        /**
         * Treats the media object as a video and loads any metadata required.
         */
        private void LoadVideo()
        {

        }

        /**
         * Treats the media object as an image and loads any metadata required.
         */
        private void LoadImage()
        {
            if (Path == null)
                return;

            // By default, don't rotate the image
            Rotation = Rotation.Rotate0;
    
            // Load image metadata
            FileStream fs = new FileStream(Path, FileMode.Open, FileAccess.Read);
            BitmapFrame frame = BitmapFrame.Create(fs, BitmapCreateOptions.DelayCreation, BitmapCacheOption.None);
            BitmapMetadata meta = frame.Metadata as BitmapMetadata;


            // Image size
            _width = frame.PixelWidth;
            _height = frame.PixelHeight;


            // Process any rotation metadata
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
