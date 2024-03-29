﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel that loads and holds one image's pixel data. Should be used by all
    /// ViewModels wanting to display an image, since the details of loading the image
    /// are taken care of here. It's important to note that setting the Photo property
    /// does not load the image into memory. To do that, the user must call LoadMedia(),
    /// which can be called asynchronously. If LoadMedia() is called multiple times
    /// asynchronously, previous calls to the method will be cancelled so that only the
    /// latest image-loading task is completed.
    /// </summary>
    class ImageViewModel : MediaViewModel
    {

        #region Constructors

        /// <summary>
        /// Creates an ImageViewModel that doesn't refer to any Photo initially.
        /// The image's preview height is set to 128, and it's target height is set to the image's full size.
        /// </summary>
        public ImageViewModel() : this(null, 128, 0) { }


        /// <summary>
        /// Creates an ImageViewModel that doesn't refer to any Photo initially.
        /// </summary>
        /// <param name="previewHeight">The height at which the image's preview will be loaded. If 0, no preview will be loaded.</param>
        /// <param name="fullHeight">The height at which the image will be loaded. If 0, the image will be loaded at its full size.</param>
        public ImageViewModel(int previewHeight, int fullHeight) : this(null, previewHeight, fullHeight) { }


        /// <summary>
        /// Creates an ImageViewModel that holds the given photo. The photo will not be loaded into memory by default, this must be done
        /// manually by calling LoadMedia()
        /// </summary>
        /// <param name="previewHeight">The height at which the image's preview will be loaded. If 0, no preview will be loaded.</param>
        /// <param name="fullHeight">The height at which the image will be loaded. If 0, the image will be loaded at its full size.</param>
        public ImageViewModel(Image photo, int previewHeight, int fullHeight)
        {
            _needReload = true;
            _previewLoaded = false;
            _media = photo;

            this._previewHeight = previewHeight;
            this._targetHeight = fullHeight;

            _cancellationTokens = new List<CancellationTokenSource>();
        }

        public override void Cleanup()
        {
            CancelLoading();
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


        /**
         * If specified, the image will be loaded twice:
         *  - first at a low resolution, the height of which is specified by _previewHeight
         *  - second at the target resolution, the height of which is specified by _targetHeight
         * 
         * Loading large images can take a long time, so the first, low-resolution load is done
         * so that the user can quickly see a blurred version of the image instead of just a blank screen.
         * 
         * If _previewHeight is set to 0, then the image will only be loaded once at the target resolution.
         * If _targetHeight is set to 0, then the target resolution will be the image's full size.
         */
        private int _previewHeight;
        private int _targetHeight;


        /**
         * Marks whether or not the image stored in the Image property is up to date with the image parameters
         * such as Photo, _previewHeight, and _targetHeight. _needReload marks whether the image has been fully
         * loaded or not (false if it has), and _previewLoaded marks whether some form of the image, preview or
         * full size, has been loaded (true if it has).
         * 
         * _needReload is used to determine if LoadMedia() needs to be called to be able to display the current
         * image, and _previewLoaded is used to determine whether the current image has been loaded enough to trigger
         * it to be displayed to the user.
         * 
         * As soon as the loaded image becomes outdated, i.e. one of the image parameters changes (what photo is
         * stored, or the sizes at which to load them, _needReload will be set to true and _previewLoaded will be
         * set to false.
         * 
         * LoadMedia() will only load the image if _needReload is true.
         */
        private bool _needReload;
        private bool _previewLoaded;


        /**
         * If the image source of this VM is switched while in the middle of loading the previous images,
         * we want to cancel those loads and only load the most recent image. This list will have one
         * CancellationTokenSource for each ongoing image load process.
         */
        private List<CancellationTokenSource> _cancellationTokens;



        /// <summary>
        /// What Photo this ViewModel should use. Changing this property will not
        /// trigger loading that photo into memory, that should be done by calling LoadMedia().
        /// </summary>
        public Image Photo
        {
            get { return _media as Image; }
            set
            {
                // If the photo is not different, don't need to update anything
                if(_media as Image != value)
                {
                    _media = value;
                    _needReload = true;
                    _previewLoaded = false;
                    CancelLoading();
                    OnPropertyChanged();
                }
            }
        }



        private ImageSource _imageData;
        /// <summary>
        /// The image information associated with the above Photo property. This is not created
        /// by default when setting the Photo, the user must call LoadMedia() separately.
        /// </summary>
        public ImageSource ImageData
        {
            get { return _imageData; }
            set
            {
                _imageData = value;
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// The aspect ratio of the image stored in the Photo property.
        /// </summary>
        public double AspectRatio
        {
            get { return Photo.AspectRatio; }
        }


        /// <summary>
        /// Whether the image stored in the Image property is up to date with the current ViewModel
        /// parameters, such as what photo it is, and preview & full resolution size. This can be used to determine
        /// whether the ViewModel is ready to be displayed to the user.
        /// </summary>
        public bool PreviewLoaded
        {
            get { return _previewLoaded; }
        }

        #endregion Fields and Properties



        #region Methods

        /// <summary>
        /// Loads this ViewModel's image (as specified by the Photo field) into memory. If
        /// specified in the properties, will first load the image at a preview resolution and
        /// then at a target resolution (see constructor parameters). If the image has previously
        /// been loaded, then it will not reload it.
        /// 
        /// This method is intended to be called asynchronously, so it will cancel any other
        /// calls to LoadMedia() that are currently loading an image.
        /// </summary>
        public override void LoadMedia()
        {
            // If the image is already loaded, do nothing.
            if (!_needReload)
            {
                OnPropertyChanged("ImageData");
                return;
            }

            // If no photo selected, nothing to load
            if (_media == null)
                return;


            // Cancel any existing image loading tasks, this should be the only one
            CancelLoading();

            // Create a cancellation token associated with loading this image
            // Add to the token list so that if this VM's image is changed, the
            // corresponding call to LoadMedia() will cancel this load task.
            CancellationTokenSource cts = new CancellationTokenSource();
            _cancellationTokens.Add(cts);



            // Store the images here temporarily, and then only update the Image
            // property if the task hasn't been cancelled
            BitmapImage tempImage;


            // Check for cancellation of load
            if (cts.IsCancellationRequested)
            {
                // Task is done, so get rid of its cancellation token
                _cancellationTokens.Remove(cts);
                cts.Dispose();
                return;
            }

            // Only load preview if a height is given
            if (_previewHeight > 0)
            {
                // LOAD IMAGE
                tempImage = LoadImage(_previewHeight);

                // Check for cancellation of load
                if (cts.IsCancellationRequested)
                {
                    // Task is done, so get rid of its cancellation token
                    _cancellationTokens.Remove(cts);
                    cts.Dispose();
                    return;
                }

                // UPDATE IMAGE PROPERTY
                ImageData = tempImage;

                _previewLoaded = true;
            }


            // Check for cancellation of load
            if (cts.IsCancellationRequested)
            {
                // Task is done, so get rid of its cancellation token
                _cancellationTokens.Remove(cts);
                cts.Dispose();
                return;
            }




            // LOAD IMAGE
            tempImage = LoadImage(_targetHeight);

            // Check for cancellation of load
            if (cts.IsCancellationRequested)
            {
                // Task is done, so get rid of its cancellation token
                _cancellationTokens.Remove(cts);
                cts.Dispose();
                return;
            }


            // UPDATE IMAGE PROPERTY
            ImageData = tempImage;


            // Now we don't need to reload the image until some parameter, either loading size or the image itself.
            _needReload = false;

            // Task is done, so get rid of its cancellation token
            _cancellationTokens.Remove(cts);
            cts.Dispose();
        }


        /**
         * Loads the VM's image into memory at the resolution specified by the given height.
         * Uses data from the Photo property to guide how to load the image.
         */
        private BitmapImage LoadImage(int height)
        {
            BitmapImage image = new BitmapImage();
            using (FileStream stream = File.OpenRead(_media.Filepath))
            {
                image.BeginInit();
                // The path could be absolute (a user submitted image) or relative (a program-generated video thumbnail file)
                //image.UriSource = new Uri(_media.Filepath, UriKind.RelativeOrAbsolute);
                image.StreamSource = stream;

                // This seems to be very important for keeping the UI responsive when asynchronously loading images
                image.CacheOption = BitmapCacheOption.OnLoad;

                // If a height is specified, only load the image at that height's corresponding resolution
                if (height > 0)
                    image.DecodePixelHeight = height;

                image.Rotation = _media.Rotation;
                image.EndInit();
            }

            // This allows _loadImage() to be called on a different thread. Normally, you can't load a BitmapImage
            // on a thread other than the main one.
            image.Freeze();

            return image;
        }



        /**
         * This is used either to cancel any outdated loading tasks when a new one is started (for example,
         * when the user goes right in the slideshow a bunch of times, only the image they land on should 
         * be loaded), or when the slideshow page is closed.
         */
        /// <summary>
        /// Cancels all current image-loading operations initiated by calling LoadMedia().
        /// </summary>
        public override void CancelLoading()
        {
            for (int i = 0; i < _cancellationTokens.Count; i++)
            {
                //TODO Is this okay? Why are they null? Seems to happen when a
                //bunch of tasks get cancelled quickly The situation has been
                //avoided by disabling image loading during batch functions.
                //But ithere probably is a better way to prevent it here.
                if (_cancellationTokens[i] != null)
                {
                    _cancellationTokens[i].Cancel();
                }

            }
        }

        #endregion Methods



        #region Static Methods

        /// <summary>
        /// Each ImageViewModel object holds one Photo object. This returns a list of the
        /// Photo objects held by a list of ImageViewModel objects.
        /// </summary>
        /// <param name="list">The list to extract Photos from.</param>
        /// <returns>A list of the Photo objects.</returns>
        public static List<Image> GetPhotoList(List<ImageViewModel> list)
        {
            List<Image> photos = new List<Image>();
            foreach (ImageViewModel ivm in list)
                photos.Add(ivm.Photo);

            return photos;
        }

        #endregion Static Methods
    }
}
