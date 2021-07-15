using PhotoGalleryApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// An abstract ViewModel class for any media type, image or video. Subclasses should be made for each media type. 
    /// </summary>
    abstract class MediaViewModel : ViewModelBase
    {
        protected Media _media;
        /// <summary>
        /// The Media object model that this view model refers to
        /// </summary>
        public Media Media
        {
            get { return _media; }
        }

        public string Filepath
        {
            get { return _media.Filepath; }
        }


        /// <summary>
        /// Returns whether or not the media is a video (true) or an image (false). Subclasses
        /// must override this to provide a return value. This is how users can distinguish between
        /// instances of this abstract class/know what subclass to cast to.
        /// </summary>
        public abstract bool IsVideo();

        /// <summary>
        /// Loads the media from disk, if the loading is not done automatically.
        /// </summary>
        public abstract void LoadMedia();

        /// <summary>
        /// Cancels any ongoing manually-triggered loading processes (any calls to LoadMedia())
        /// </summary>
        public abstract void CancelLoading();


        #region Static

        /// <summary>
        /// From a list of MediaViewModel objects, returns a list of the Media objects that they contain.
        /// </summary>
        /// <param name="list">The list of MediaViewModel objects to compile from.</param>
        /// <returns>A list of the Media objects stored in the MediaViewModel list.</returns>
        public static List<Media> GetMediaList(List<MediaViewModel> list)
        {
            List<Media> photos = new List<Media>();
            foreach (MediaViewModel ivm in list)
                photos.Add(ivm.Media);

            return photos;
        }

        #endregion Static
    }
}
