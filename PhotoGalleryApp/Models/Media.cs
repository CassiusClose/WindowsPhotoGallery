using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xml.Serialization;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// Represents one piece of media, photo or image, associated with a list of tags.
    /// </summary>
    /**
     * List possible subclasses of the abstract class for the serializer
     */
    [XmlInclude(typeof(Image))]
    [XmlInclude(typeof(Video))]
    public abstract class Media
    {
        #region Constructors

        /// <summary>
        /// Creates a media object with the given path, with the given list of tags.
        /// </summary>
        /// <param name="path">The filepath of the media object.</param>
        /// <param name="tags">A list of tags that the media object should have.</param>
        public Media(string path, ObservableCollection<string> tags)
        {
            Filepath = path;

            if (tags == null)
                Tags = new ObservableCollection<string>();
            else
                Tags = tags;
        }


        /// <summary>
        /// Creates a media object with the given filepath.
        /// </summary>
        /// <param name="path">The filepath of the media file.</param>
        public Media(string path) : this(path, null) { }
        
        
        /**
         * Need by XmlSerializer
         */
        protected Media() : this(null, null) { }

        #endregion Constructors



        #region Fields and Properties


        protected string _filepath;
        /// <summary>
        /// The filepath of the piece of media.
        /// </summary>
        public string Filepath 
        {
            get { return _filepath; }
            set
            {
                if (value == null)
                    return;

                if (!Path.HasExtension(value))
                    return;

                _filepath = value;

                if (value != null)
                {
                    _extension = Path.GetExtension(_filepath).ToLower();

                    if(IsVideo() && (Extension != ".mp4"))
                        throw new Exception("Tried to set an image file as the path of a Video model."); 
                    else if(!IsVideo() && (Extension != ".jpg" && Extension != ".jpeg" && Extension != ".png"))
                        throw new Exception("Tried to set an video file as the path of a Image model.");

                    // Load the media's metadata
                    LoadMetadata();
                }
            }
        }

        /// <summary>
        /// Whether the media object is a video (true) or an image (false)
        /// </summary>
        public abstract bool IsVideo();



        protected string _extension;
        /// <summary>
        /// The media file's extension
        /// </summary>
        public string Extension { get { return _extension; } }


        /// <summary>
        /// How much the image should be rotated.
        /// </summary>
        public Rotation Rotation { get; set; }


        protected int _width;
        /// <summary>
        /// The width of the image
        /// </summary>
        public int Width 
        {
            get { return _width; }
        }

        protected int _height;
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
         * Loads the media file's metadata. Subclasses should implement this depending on the media file type.
         */
        protected abstract void LoadMetadata();


        /// <summary>
        /// Returns a string representation of this photo.
        /// </summary>
        /// <returns>A string representation of this photo.</returns>
        public override string ToString()
        {
            return Filepath;
        }

        #endregion Methods
    }
}
