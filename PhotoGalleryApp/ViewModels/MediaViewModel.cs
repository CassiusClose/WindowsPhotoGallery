using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// An abstract ViewModel class for any media type, image or video. Subclasses should be made for each media type. 
    /// </summary>
    abstract class MediaViewModel : ICollectableViewModel
    {
        #region Fields and Properties

        protected Media _media;
        /// <summary>
        /// The Media object model that this view model refers to
        /// </summary>
        public Media Media
        {
            get { return _media; }
        }

        /// <summary>
        /// The filepath of the media object this VM holds
        /// </summary>
        public string Filepath
        {
            get { return Media.Filepath; }
        }

        /// <summary>
        /// The DateTime that the media was created at
        /// </summary>
        public DateTime Timestamp
        {
            get { return Media.Timestamp; }
        }

        /// <summary>
        /// The type of media this VM holds (video, image, etc.)
        /// </summary>
        public MediaFileType MediaType
        {
            get { return Media.MediaType; }
        }

        public Rotation Rotation
        {
            get { return Media.Rotation; }
        }

        /*
         * Whether the Media is selected in whatever parent container. I think this should be
         * fine even if the Media is owned by several different parents, because each one will
         * have a different MediaViewModel for the same Media, and hence different IsSelected
         * properties.
         */

        #endregion Fields and Properties

        /// <summary>
        /// Returns the ICollectable model associated with this viewmodel
        /// </summary>
        /// <returns>The ICollectable model associated with this viewmodel</returns>
        public override abstract ICollectable GetModel();

        protected override DateTime _getTimestamp()
        {
            return Media.Timestamp;
        }


        #region Methods

        /// <summary>
        /// Loads the media from disk, if the loading is not done automatically.
        /// </summary>
        public abstract void LoadMedia();

        /// <summary>
        /// Cancels any ongoing manually-triggered loading processes (any calls to LoadMedia())
        /// </summary>
        public abstract void CancelLoading();

        #endregion Methods



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


        /// <summary>
        /// Creates and returns a MediaViewModel object, that holds the given Media object. Will be
        /// either an ImageViewModel or VideoViewModel instance, depending on the type of media the
        /// Media object contains.
        /// </summary>
        /// <param name="media">The Media object that the MediaViewModel will contain.</param>
        /// <returns>A MediaViewModel which contains the Media object, either an ImageViewModel
        /// or VideoViewModel.</returns>
        public static MediaViewModel CreateMediaViewModel(Media media, bool videoThumbnailMode, int previewHeight, int fullHeight)
        {
            switch(media.MediaType)
            {
                case MediaFileType.Video:               
                    VideoViewModel vvm = new VideoViewModel(media as Video, videoThumbnailMode, previewHeight, fullHeight);
                    return vvm;

                case MediaFileType.Image:
                    ImageViewModel ivm = new ImageViewModel(media as Image, previewHeight, fullHeight);
                    return ivm;

                default:
                    return null;
            }
        }

        #endregion Static
    }
}
