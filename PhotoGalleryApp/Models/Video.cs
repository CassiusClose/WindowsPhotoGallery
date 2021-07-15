using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.Models
{
    /// <summary>
    /// A Media object that refers to a video file.
    /// </summary>
    public class Video : Media
    {
        #region Constructors

        /// <summary>
        /// Creates an Image object with the given filepath.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        public Video(string path) : base(path) { }

        /// <summary>
        /// Creates an Image object with the given path, with the given list of tags.
        /// </summary>
        /// <param name="path">The filepath of the image.</param>
        /// <param name="tags">A list of tags that the image should have.</param>
        public Video(string path, ObservableCollection<string> tags) : base(path, tags) { }

        /**
         * Needed by XmlSerializer
         */
        protected Video() : base() { }

        #endregion Constructors


        private string _thumbnailPath;
        public string ThumbnailPath
        {
            get { return _thumbnailPath; }
        }


        protected override void LoadMetadata()
        {
            _thumbnailPath = Path.GetFileNameWithoutExtension(Filepath);

            // Search directory for conflicting images, add numbers if conflict

            // Invoke ffmpeg to create a thumbnail
        }

        /// <summary>
        /// Returns whether the media object is a video (true) or an image (false). This
        /// will always return true. This is used to determine which Media subclass
        /// a Media instance belongs to.
        /// </summary>
        public override bool IsVideo()
        {
            return true;
        }
    }
}
