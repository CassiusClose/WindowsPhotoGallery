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
    abstract class MediaViewModel : ViewModelBase
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
        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; } 
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        #endregion Fields and Properties



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

        #endregion Static
    }
}
