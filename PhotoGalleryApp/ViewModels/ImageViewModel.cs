using System;
using System.Collections.Generic;
using System.Linq;
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
    /// does not load the image into memory. To do that, the user must call UpdateImage(),
    /// which can be called asynchronously. If UpdateImage() is called multiple times
    /// asynchronously, previous calls to the method will be cancelled so that only the
    /// latest image-loading task is completed.
    /// </summary>
    class ImageViewModel : ViewModelBase
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
        /// manually by calling UpdateImage()
        /// </summary>
        /// <param name="previewHeight">The height at which the image's preview will be loaded. If 0, no preview will be loaded.</param>
        /// <param name="fullHeight">The height at which the image will be loaded. If 0, the image will be loaded at its full size.</param>
        public ImageViewModel(Photo photo, int previewHeight, int fullHeight)
        {
            this._previewHeight = previewHeight;
            this._targetHeight = fullHeight;

            cancellationTokens = new List<CancellationTokenSource>();
            
            _photo = photo;
            _needReload = true;
            _previewLoaded = false;
        }


        #endregion Constructors



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
         * _needReload is used to determine if UpdateImage() needs to be called to be able to display the current
         * image, and _previewLoaded is used to determine whether the current image has been loaded enough to trigger
         * it to be displayed to the user.
         * 
         * As soon as the loaded image becomes outdated, i.e. one of the image parameters changes (what photo is
         * stored, or the sizes at which to load them, _needReload will be set to true and _previewLoaded will be
         * set to false.
         * 
         * UpdateImage() will only load the image if _needReload is true.
         */
        private bool _needReload;
        private bool _previewLoaded;


        /**
         * If the image source of this VM is switched while in the middle of loading the previous images,
         * we want to cancel those loads and only load the most recent image. This list will have one
         * CancellationTokenSource for each ongoing image load process.
         */
        private List<CancellationTokenSource> cancellationTokens;



        private Photo _photo;
        /// <summary>
        /// What Photo this ViewModel should use. Changing this property will not
        /// trigger loading that photo into memory, that should be done by calling UpdateImage().
        /// </summary>
        public Photo Photo
        {
            get { return _photo; }
            set
            {
                // If the photo is not different, don't need to update anything
                if(_photo != value)
                {
                    _photo = value;
                    _needReload = true;
                    _previewLoaded = false;
                    CancelAllLoads();
                    OnPropertyChanged();
                }
            }
        }



        private ImageSource _image;
        /// <summary>
        /// The image information associated with the above Photo property. This is not created
        /// by default when setting the Photo, the user must call UpdateImage() separately.
        /// </summary>
        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
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
        /// Returns whether or not the image stored in the Image property is up to date with the current ViewModel
        /// parameters, such as what photo it is, and preview & full resolution size. This can be used to determine
        /// whether the ViewModel is ready to be displayed to the user.
        /// </summary>
        /// <returns>Whether or not the Image property is up to date with this ViewModel's parameters.</returns>
        public bool PreviewLoaded()
        {
            return _previewLoaded;
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
        /// calls to UpdateImage() that are currently loading an image.
        /// </summary>
        public void UpdateImage()
        {

            // If the image is already loaded, do nothing.
            if (!_needReload)
            {
                OnPropertyChanged("Image");
                return;
            }

            // If no photo selected, nothing to load
            if (_photo == null)
                return;


            // Cancel any existing image loading tasks, this should be the only one
            CancelAllLoads();

            // Create a cancellation token associated with loading this image
            // Add to the token list so that if this VM's image is changed, the
            // corresponding call to UpdateImage() will cancel this load task.
            CancellationTokenSource cts = new CancellationTokenSource();
            cancellationTokens.Add(cts);



            // Store the images here temporarily, and then only update the Image
            // property if the task hasn't been cancelled
            BitmapImage tempImage;


            // Check for cancellation of load
            if (cts.IsCancellationRequested)
            {
                // Task is done, so get rid of its cancellation token
                cancellationTokens.Remove(cts);
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
                    cancellationTokens.Remove(cts);
                    cts.Dispose();
                    return;
                }

                // UPDATE IMAGE PROPERTY
                Image = tempImage;

                _previewLoaded = true;
            }


            // Check for cancellation of load
            if (cts.IsCancellationRequested)
            {
                // Task is done, so get rid of its cancellation token
                cancellationTokens.Remove(cts);
                cts.Dispose();
                return;
            }



            // LOAD IMAGE
            tempImage = LoadImage(_targetHeight);

            // Check for cancellation of load
            if (cts.IsCancellationRequested)
            {
                // Task is done, so get rid of its cancellation token
                cancellationTokens.Remove(cts);
                cts.Dispose();
                return;
            }


            // UPDATE IMAGE PROPERTY
            Image = tempImage;


            // Now we don't need to reload the image until some parameter, either loading size or the image itself.
            _needReload = false;

            // Task is done, so get rid of its cancellation token
            cancellationTokens.Remove(cts);
            cts.Dispose();
        }



        /**
         * Loads the VM's image into memory at the resolution specified by the given height.
         * Uses data from the Photo property to guide how to load the image.
         */
        private BitmapImage LoadImage(int height)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(_photo.Path);

            // This seems to be very important for keeping the UI responsive when asynchronously loading images
            image.CacheOption = BitmapCacheOption.OnLoad;

            // If a height is specified, only load the image at that height's corresponding resolution
            if (height > 0)
                image.DecodePixelHeight = height;

            image.Rotation = _photo.Rotation;
            image.EndInit();

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
        /// Cancels all current image-loading operations initiated by calling UpdateImage().
        /// </summary>
        public void CancelAllLoads()
        {
            for (int i = 0; i < cancellationTokens.Count; i++)
                cancellationTokens[i].Cancel();
        }

        #endregion Methods



        #region Static Methods

        /// <summary>
        /// Each ImageViewModel object holds one Photo object. This returns a list of the
        /// Photo objects held by a list of ImageViewModel objects.
        /// </summary>
        /// <param name="list">The list to extract Photos from.</param>
        /// <returns>A list of the Photo objects.</returns>
        public static List<Photo> GetPhotoList(List<ImageViewModel> list)
        {
            List<Photo> photos = new List<Photo>();
            foreach (ImageViewModel ivm in list)
                photos.Add(ivm.Photo);

            return photos;
        } 

        #endregion Static Methods
    }
}
