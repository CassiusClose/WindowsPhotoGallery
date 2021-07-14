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

        public VideoViewModel(Media video)
        {
            if (!video.IsVideo)
                throw new Exception("Tried to set a image Media model to a VideoViewModel");

            _media = video;
        }

        #endregion Constructors


        #region Fields and Properties

        /// <summary>
        /// Returns the Media object that this ViewModel refers too.
        /// </summary>
        public Media Video
        {
            get { return _media; }
            set
            {
                if(!value.IsVideo)
                    throw new Exception("Tried to set a image Media model to a VideoViewModel");

                if(_media != value)
                {
                    _media = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The filepath that the video is stored at
        /// </summary>
        public string Path
        {
            get { return Video.Path; }
        }

        #endregion Fields and Properties


        #region Methods

        /**
         * Any instance of this class will represent an image, not an object. This is here
         * to distinguish between other subclasses of MediaViewModel.
         */
        protected override bool MediaIsVideo()
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
