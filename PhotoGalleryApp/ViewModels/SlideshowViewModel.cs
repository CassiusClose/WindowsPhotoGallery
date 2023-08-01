using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhotoGalleryApp.Models;
using PhotoGalleryApp.Utils;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel that displays one image at a time from a collection of images
    /// </summary>
    /// <example>
    /// From a gallery view of photos, the user can click on one to view it at a
    /// larger resolution. From that view, the user can navigate left and right to
    /// view the other images in the gallery at a large resolution. This ViewModel
    /// would control that larger-resolution view.
    /// </example>
    class SlideshowViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Creates a large image-viewer from the list of photos given.
        /// </summary>
        /// <param name="galleryItems">The Photos available to view in this vm.</param>
        /// <param name="index">The index of the currently selected Photo in the given list.</param>
        /// <param name="gallery">The PhotoGallery that these photos belong to.</param>
        //TODO A simple list isn't good enough here. What if stuff changes?
        public SlideshowViewModel(List<Media> galleryItems, int index, MediaCollection gallery)
        {
            // Initialize commands
            _leftCommand = new RelayCommand(Left, HasMultipleImages);
            _rightCommand = new RelayCommand(Right, HasMultipleImages);
            _toggleInfoVisibilityCommand = new RelayCommand(ToggleInfoVisibility);

            
            _galleryItems = galleryItems;
            CurrentIndex = index;

            // Save a reference to the photo gallery because MediaInfoViewModel needs
            // it, and those could be created every time the user goes to a new image.
            _gallery = gallery;


            // Setup image cache and start loading the images
            InitCache();

            CurrentMediaChanged();

            LoadWholeCache();
        }


        #endregion Constructors



        #region Fields and Properties

        /*
         * A collection of images that are viewable from this page. The user can cycle between these images.
         * TODO This can be outdated - if somehow the gallery contents change while viewing these images, this list will
         * not reflect those changes. That's something to fix in the future.
         */
        private List<Media> _galleryItems;
        
        /*
         * The index of the currently selected image from the viewable list of images.
         */
        private int CurrentIndex;


        /**
        * The PhotoGallery that the viewable collection of images here belong to. This is
        * needed to instantiate MediaInfoViewModels, so it is saved here.
        */
        private MediaCollection _gallery;



        /// <summary>
        /// The ViewModel that holds the currently-selected image to display.
        /// </summary>
        public MediaViewModel CurrentMediaViewModel
        {
            get { return _imageCache[_imageCacheCurrIndex]; }
        }



        private ViewModelBase _sidebarContent;
        /// <summary>
        /// Holds the ViewModel whose data is displayed on the sidebar.
        /// </summary>
        public ViewModelBase SidebarContent
        {
            get { return _sidebarContent; }
            set
            {
                _sidebarContent = value;
                OnPropertyChanged();
            }
        }


        private static bool _sidebarVisible = false;
        /// <summary>
        /// Whether or not the sidebar is visible to the user.
        /// </summary>
        public bool SidebarVisible
        {
            get { return _sidebarVisible; }
            set 
            {
                _sidebarVisible = value;
                OnPropertyChanged();
            }
        }



        #endregion Fields and Properties



        #region Methods


        #region Change Handlers

        /**
         * Called when a child view model's properties get changed. Specifically, this class should subscribe to property changes of child
         * ImageViewModel instances in the cache. I was having trouble when I had the CurrentImage property returning the current ImageViewModel
         * instead of that view model's Image property. Somehow, when navigating between images, the binding would get confused about which 
         * ImageViewModel instance to bind to, and would change to the wrong image. So now, CurrentImage refers to the Image property within the 
         * ImageViewModel. For this to work, the CurrentImage property here must broadcast a change to its listeners when the Image property
         * within the ImageViewModel changes. So this class subscribes to the ImageViewModel's property changes and if Image is changed,
         * notifies CurrentImage listeners of a change.
         */
        private void Child_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ImageData")
            {
                // Only update if the current image is the one that's changed
                ImageViewModel vm = sender as ImageViewModel;
                if (vm == CurrentMediaViewModel)
                {
                    OnPropertyChanged("CurrentMediaViewModel");
                }
            }
        }


        /**
         * Updates data related to the current image when the image changes. This isn't called when the image loads, just when the photo selected
         * is changed. Updates sidebar data, for example.
         */
        private void CurrentMediaChanged()
        {
            if(SidebarVisible)
            {
                SidebarContent = new MediaInfoViewModel(CurrentMediaViewModel.Media, _gallery);
            }
        }



        #endregion Change Handlers


        #region Misc

        /// <summary>
        /// Called when this page is no longer visible to the user. This cancels any image loading operations that might be going on.
        /// </summary>
        public override void NavigatorLostFocus()
        {
            foreach (MediaViewModel vm in _imageCache)
                vm.CancelLoading();
        }

        #endregion Misc


        #endregion Methods



        #region Image Caching



        /** IMAGE CACHING
         * Not only does this store the current image the user is looking at, it also loads and stores several images just
         * before and just after the current image. This takes advantage of any time that the user is paused to look at an
         * image. While they are looking, the nearby images will be loaded in the background, to decrease loading times when
         * the user switches to the next image.
         * 
         * Because the user will only ever be switching left or right one image at a time, this uses a technique similar to
         * a circular buffer in that the position of the current image changes over time and the images relative to it can
         * wrap from the end to the beginning of the array. The images are stored in the cache in the same order they are in
         * the gallery: the previous images are stored before the current image, and the next images are stored after the
         * current image.
         * 
         * Wherever the current image is stored in the cache, when the user moves one image to the right, all the caching
         * methods have to do is move the current image index of the cache to the right by one. The number of images cached
         * before and after the current image are constant, so when moving left or right, we only have to update one cache
         * position to store a new image. 
         */

        // Cache sizes. How many images before and the current image to cache, and the total cache length.
        private int BACK_CACHE_NUM;
        private int FORWARD_CACHE_NUM;
        private int CACHE_LEN;

        // The cache itself and the position of the current image within it
        private MediaViewModel[] _imageCache;
        private int _imageCacheCurrIndex = 0;



        /**
        * Initializes the cache of images as an array. Initially, the index of the current image is the 2nd position
        * in the array. As the user moves left & right through the slideshow, the index will move left & right along
        * with it, wrapping at the end as with a circular buffer. The cached images nearby the current image will be
        * in the same relative position to the current image's position in the array.
        */
        private void InitCache()
        {
            /* Determine Cache Length
             * The cache has a default length, which it will be, unless the number of items to display in the slideshow
             * is smaller than that length. In that case, the cache will be sized to fit the number of items.
             */
            BACK_CACHE_NUM = 1;
            FORWARD_CACHE_NUM = 2;
            CACHE_LEN = BACK_CACHE_NUM + FORWARD_CACHE_NUM + 1;

            // If fewer items than the cache size
            if (_galleryItems.Count < CACHE_LEN)
            {
                // The size of the cache that holds non-current items (items before and after the current item)
                int nonCurrCacheNum = _galleryItems.Count - 1;

                // If even number of cache positions left, divide evenly between back & forward cache
                if (nonCurrCacheNum % 2 == 0)
                {
                    BACK_CACHE_NUM = nonCurrCacheNum / 2;
                    FORWARD_CACHE_NUM = nonCurrCacheNum / 2;
                }
                // Otherwise, give 1 extra cache position to the forward cache
                else
                {
                    BACK_CACHE_NUM = (nonCurrCacheNum - 1) / 2;
                    FORWARD_CACHE_NUM = nonCurrCacheNum - BACK_CACHE_NUM;
                }

                CACHE_LEN = BACK_CACHE_NUM + FORWARD_CACHE_NUM + 1;
            }

            // Init the cache array itself
            _imageCache = new MediaViewModel[CACHE_LEN];



            /* 
             * Initialize each position in the cache
             */

            int cacheIndex = 0;

            // Create each cache position for images before the current one
            for (int i = 1; i <= BACK_CACHE_NUM; i++)
            {
                // Which image in the gallery do we load into the cache
                int galleryIndex = CurrentIndex - i;
                if (galleryIndex < 0)
                    galleryIndex += _galleryItems.Count();

                // Create the VM which holds the image & subscribe to property change events
                _imageCache[cacheIndex++] = CreateMediaViewModel(_galleryItems[galleryIndex]);
            }


            // Create a cache position for the current image
            _imageCacheCurrIndex = cacheIndex;
            // Create the VM which holds the image & subscribe to property change events
            _imageCache[cacheIndex++] = CreateMediaViewModel(_galleryItems[CurrentIndex]);


            // Create each cache position for images after the current one
            for (int i = 1; i <= FORWARD_CACHE_NUM; i++)
            {
                // Which image in the gallery do we load into the cache
                int galleryIndex = CurrentIndex + i;
                if (galleryIndex >= _galleryItems.Count())
                    galleryIndex -= _galleryItems.Count();

                // Create the VM which holds the image & subscribe to property change events
                _imageCache[cacheIndex++] = CreateMediaViewModel(_galleryItems[galleryIndex]);
            }
        }


        /* 
         * Creates and returns a MediaViewModel that holds the given Media object. Initializes the
         * view model with settings specific to this slideshow's view. Depending on the type of media
         * file within the Media object, the returned object will either be an ImageViewModel or a
         * VideoViewModel.
         */
        private MediaViewModel CreateMediaViewModel(Media media)
        {
            switch (media.MediaType)
            {
                case MediaFileType.Video:
                    return new VideoViewModel(media as Video);

                case MediaFileType.Image:
            
                    //Load images at 256 pixel thumbnail resolution, then at full size
                    ImageViewModel imageVM = new ImageViewModel(media as Image, 256, 0);
                    imageVM.PropertyChanged += new PropertyChangedEventHandler(Child_OnPropertyChanged);
                    return imageVM;

                default:
                    return null;
            }
        }



        /**
         * Triggers the entire cache to load their images. If the images are already loaded, they will not be reloaded.
         * This starts by loading the current image, the images after it, and then the images before it.
         */
        private async void LoadWholeCache()
        {
            // Save a copy of the current index here. This way, if the user moves left or right
            // while this method is executing, which happens often when the slideshow has just
            // been displayed, this method will continue load the proper cache positions.
            int currIndex = _imageCacheCurrIndex;

            // Here, i is the number of positions after the index of the current image in the cache
            for (int i = 0; i < CACHE_LEN; i++)
            {
                // Calculate which cache position to load
                int index = currIndex + i;
                if (index >= CACHE_LEN)
                    index -= CACHE_LEN;

                // Load the image
                await Task.Run(() => { _imageCache[index].LoadMedia(); });
            }
        }


        /**
         * Switches the current image one to the left. The means changing the index of the current image within
         * the cache and also updating one of the cache positions. Because the cache always holds the same number of
         * images before and the same number after the current image, moving the current image left means that one of
         * cache positions after the current image will need to store an image before the current one.
         * 
         * Once this is done, the cached images will be loaded according to priority. From highest to lowest priority,
         * this is the current image, the ones after it, and the ones before it. This corresponds to how likely it is
         * the user will view each image.
         */
        private async void MoveCacheLeft()
        {
            // First, move the current image index one to the left
            _imageCacheCurrIndex--;
            if (_imageCacheCurrIndex < 0)
                _imageCacheCurrIndex += CACHE_LEN;


            // Update sidebar first so that it's not waiting on the image load to update
            CurrentMediaChanged();


            /* If the cache size is exactly the number of items possible to display, then all of the items should be
             * loaded, and all we need to do is move the cache position over and be done with it.
             * 
             * If it's not, then we must update on cache position so that the cache centers around the new current item.
             */
            int cacheUpdateIndex = 0;
            if (_galleryItems.Count != _imageCache.Length)
            {
                cacheUpdateIndex = _imageCacheCurrIndex - BACK_CACHE_NUM;

                // Which position in the cache is now stale and should be replaced
                if (cacheUpdateIndex < 0)
                    cacheUpdateIndex += CACHE_LEN;

                // What image in the gallery will replace the stale cache position
                int galleryUpdateIndex = CurrentIndex - BACK_CACHE_NUM;
                if (galleryUpdateIndex < 0)
                    galleryUpdateIndex += _galleryItems.Count();

                // Update the cache position with the new image
                Media p = _galleryItems[galleryUpdateIndex];
                _imageCache[cacheUpdateIndex] = CreateMediaViewModel(p);
            }



            /* If the new current item is fully loaded, then trigger a display update. If not,
             * then when it's loaded, that will trigger a display update in the callback elsewhere
             * in this class.
             */

            // We don't have an event that gets called when the video is loaded, so just
            // treat it as if it's loaded, and if it's not then the screen will be blank
            // for a second.
            switch (CurrentMediaViewModel.MediaType)
            {
                case MediaFileType.Video:
                    OnPropertyChanged("CurrentMediaViewModel");
                    break;

                case MediaFileType.Image:
                    // If the current image has been loaded (i.e., there isn't an old image
                    // still loaded), either the preview or the full resolution, then update the
                    // view to the user. Otherwise, when the image loads, the view will update.
                    if ((CurrentMediaViewModel as ImageViewModel).PreviewLoaded)
                        OnPropertyChanged("CurrentMediaViewModel");
                    break;
            }



            // If a cache position was updated with a new item, then load the item's data
            if (_galleryItems.Count != _imageCache.Length)
            {
                // Then load the new cache position's image
                await Task.Run(() => { _imageCache[cacheUpdateIndex].LoadMedia(); });
            }
        }



        /**
        * Switches the current image one to the right. The means changing the index of the current image within
        * the cache and also updating one of the cache positions. Because the cache always holds the same number of
        * images before and the same number after the current image, moving the current image right means that one of
        * cache positions before the current image will need to store an image after the current one.
        * 
        * Once this is done, the cached images will be loaded according to priority. From highest to lowest priority,
        * this is the current image, the ones after it, and the ones before it. This corresponds to how likely it is
        * the user will view each image.
        */
        private async void MoveCacheRight()
        {
            // First, move the current image index one to the right
            _imageCacheCurrIndex++;
            if (_imageCacheCurrIndex >= CACHE_LEN)
                _imageCacheCurrIndex -= CACHE_LEN;


            // Update sidebar first so that it's not waiting on the image load to update
            CurrentMediaChanged();

            if (_galleryItems.Count == _imageCache.Length)
            {
                OnPropertyChanged("CurrentMediaViewModel");
                return;
            }


            /* If the cache size is exactly the number of items possible to display, then all of the items should be
             * loaded, and all we need to do is move the cache position over and be done with it.
             * 
             * If it's not, then we must update on cache position so that the cache centers around the new current item.
             */
            int cacheUpdateIndex = 0;
            if (_galleryItems.Count != _imageCache.Length)
            {
                // Which position in the cache is now stale and should be replaced
                cacheUpdateIndex = _imageCacheCurrIndex + FORWARD_CACHE_NUM;
                if (cacheUpdateIndex >= CACHE_LEN)
                    cacheUpdateIndex -= CACHE_LEN;

                int galleryUpdateIndex = CurrentIndex + FORWARD_CACHE_NUM;
                if (galleryUpdateIndex >= _galleryItems.Count())
                    galleryUpdateIndex -= _galleryItems.Count();

                // Update the cache position with the new image
                Media p = _galleryItems[galleryUpdateIndex];
                _imageCache[cacheUpdateIndex] = CreateMediaViewModel(p);
            }



            /* If the new current item is fully loaded, then trigger a display update. If not,
             * then when it's loaded, that will trigger a display update in the callback elsewhere
             * in this class.
             */

            // We don't have an event that gets called when the video is loaded, so just
            // treat it as if it's loaded, and if it's not then the screen will be blank
            // for a second.
            switch (CurrentMediaViewModel.MediaType)
            {
                case MediaFileType.Video:
                    OnPropertyChanged("CurrentMediaViewModel");
                    break;

                case MediaFileType.Image:
                    // If the current image has been loaded (i.e., there isn't an old image
                    // still loaded), either the preview or the full resolution, then update the
                    // view to the user. Otherwise, when the image loads, the view will update.
                    if ((CurrentMediaViewModel as ImageViewModel).PreviewLoaded)
                        OnPropertyChanged("CurrentMediaViewModel");
                    break;
            }




            // If a cache position was updated with a new item, then load the item's data
            if (_galleryItems.Count != _imageCache.Length)
            {
                // Then load the new cache position's image
                await Task.Run(() => { _imageCache[cacheUpdateIndex].LoadMedia(); });
            }
        }


        #endregion Image Caching



        #region Commands

        private RelayCommand _leftCommand;
        /// <summary>
        /// A command that switches the display to the image before (or to the left of) the current image.
        /// </summary>
        public ICommand LeftCommand => _leftCommand;

        /// <summary>
        /// Switches the display to the image before (or to the left of) the current image.
        /// </summary>
        public void Left()
        {
            CurrentIndex--;
            // Wrap to end
            if (CurrentIndex < 0)
                CurrentIndex = _galleryItems.Count - 1;

            MoveCacheLeft();
        }


        private RelayCommand _rightCommand;
        /// <summary>
        /// A command that switches the display to the image after (or to the right of) the current image.
        /// </summary>
        public ICommand RightCommand => _rightCommand;

        /// <summary>
        /// Switches the display to the image after (or to the right of) the current image.
        /// </summary>
        public void Right()
        {
            CurrentIndex++;
            // Wrap to beginning
            if (CurrentIndex >= _galleryItems.Count)
                CurrentIndex = 0;

            MoveCacheRight();
        }


        /**
         * Used to determine whether the left & right commands should be usable.
         */
        private bool HasMultipleImages()
        {
            return _galleryItems.Count > 1;
        }




        private RelayCommand _toggleInfoVisibilityCommand;
        /// <summary>
        /// A command that toggles the visibility of the info sidebar. If the info pane is hidden, it will be
        /// made visible, and vice versa.
        /// </summary>
        public ICommand ToggleInfoVisibilityCommand => _toggleInfoVisibilityCommand;

        /// <summary>
        /// Toggles the visibility of the info sidebar. If the info pane is hidden, it will be made visible,
        /// and vice versa.
        /// </summary>
        public void ToggleInfoVisibility()
        {
            SidebarVisible = !SidebarVisible;
            
            // Update sidebar info, which will happen only if the info is visible
            CurrentMediaChanged();
        }


        #endregion Commands
    }
}   
