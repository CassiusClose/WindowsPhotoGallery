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

        /// <summary>
        /// Creates a view model that refers to the given Video.
        /// Optionally, can be put in ThumbnailMode, which means that users of this ViewModel will be
        /// expecting to use the thumbnail of this video. An ImageViewModel will be created for the
        /// thumbnail, which will be an accessible property. The ImageViewModel is passed the optional
        /// height arguments to this constructor.
        /// </summary>
        /// <param name="video">The Video object this view model will refer to.</param>
        /// <param name="thumbnailMode">Optional (default false), whether thumbnail mode should be active (i.e. whether the
        /// thumbnail of the video will need to be used as an ImageViewModel).</param>
        /// <param name="thumbnailPreviewHeight">Optional, (default 0), if thumbnail mode is active, the preview height
        /// that will be passed to the constructor of the thumbnail's ImageViewModel.</param>
        /// <param name="thumbnailFinalHeight">Optional, (default 0), if thumbnail mode is active, the final height that
        /// will be passed to the constructor of the thumbnail's ImageViewModel.</param>
        public VideoViewModel(Video video, bool thumbnailMode = false, int thumbnailPreviewHeight = 0, int thumbnailFinalHeight = 0)
        {
            _media = video;

            ThumbnailMode = thumbnailMode;

            // If thumbnail mode is activated, create a view model for the thumbnail
            if(ThumbnailMode)
            {
                // The thumbnail is loaded here so that when the XmlDeserializer creates a Video object, it doesn't attempt to
                // load the thumbnail until the deserialization is complete. Otherwise, the Video object would load a new
                // thumbnail before its Thumbnail property is set by the deserializer.
                video.LoadThumbnail();
                _thumbnailViewModel = new ImageViewModel(video.Thumbnail, thumbnailPreviewHeight, thumbnailFinalHeight);
            }
        }

        #endregion Constructors

        /// <summary>
        /// Returns the ICollectable model associated with this viewmodel
        /// </summary>
        /// <returns>The ICollectable model associated with this viewmodel</returns>
        public override ICollectable GetModel()
        {
            return Media;
        }

        #region Fields and Properties

        /// <summary>
        /// Whether or not Thumbnail Mode is active. In Thumbnail Mode, users of this view model
        /// need to use/display this video's thumbnail. An ImageViewModel is created in the
        /// ThumbnailViewModel property and updated accordingly when this view model's LoadMedia()
        /// and CancelLoading() is called.
        /// Users that do not need to use/display the thumbnail should not enable ThumbnailMode, as
        /// it will save the overhead of intializing/loading the thumbnail.
        /// </summary>
        public bool ThumbnailMode { get; private set; }


        private ImageViewModel _thumbnailViewModel;
        /// <summary>
        /// The ImageViewModel associated with this video's thumbnail. Trying to get this property
        /// when Thumbnail Mode is not active will throw an error.
        /// </summary>
        public ImageViewModel ThumbnailViewModel
        {
            get 
            {
                if (!ThumbnailMode)
                    throw new Exception("Requested ThumbnailViewModel within a VideoViewModel that is not in Thumbnail Mode");

                return _thumbnailViewModel;
            }
        }


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

        /**
         * Triggers manual loading of the video. The video is loaded automatically by WPF, so
         * nothing is done here.
         */
        /// <summary>
        /// Triggers a manual loading of data. The video is loaded automatically by WPF, so nothing is done
        /// for that, but if Thumbnail Mode is enabled, then this will trigger loading of the thumbnail.
        /// </summary>
        public override void LoadMedia()
        {
            if (ThumbnailMode)
                ThumbnailViewModel.LoadMedia();
        }

        /**
         * Cancels any manual loading tasks. If ThumbnailMode is enabled, this will cancel any loading tasks
         * started by LoadMedia().
         */
        public override void CancelLoading()
        {
            if (ThumbnailMode)
                ThumbnailViewModel.CancelLoading();
        }

        #endregion Methods
    }
}
