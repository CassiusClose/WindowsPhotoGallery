using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{  
    /// <summary>
    /// A ViewModel which holds the data about one video file. 
    /// </summary>
    class VideoViewModel : MediaViewModel
    {
        #region Constructors

        public VideoViewModel(Video video)
        {
            _media = video;
        }

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// Returns the Media object that this ViewModel refers too.
        /// </summary>
        public Video Video
        {
            get { return _media as Video; }
            set
            {
                if(_media as Video != value)
                {
                    _media = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion Fields and Properties


        #region Methods

        /// <summary>
        /// Returns whether or not the media is a video (true) or an image (false). This will always
        /// return true. This is how users can distinguish between the types of instances of MediaViewModel.
        /// </summary>
        public override bool IsVideo()
        {
            return true;
        }

        /**
         * Triggers manual loading of the video. The video is loaded automatically by WPF, so
         * nothing is done here.
         */
        public override void LoadMedia() { }

        /**
         * Cancels any manual loading tasks. LoadMedia() does nothing, so nothing here to cancel.
         */
        public override void CancelLoading() { }

        #endregion Methods
    }
}
