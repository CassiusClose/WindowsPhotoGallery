using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PhotoGalleryApp.Models;

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
    class SingleImageViewModel : ViewModelBase
    {
        #region Constructors

        /// <summary>
        /// Creates a large image-viewer from the list of photos given.
        /// </summary>
        /// <param name="galleryItems">The Photos available to view in this vm.</param>
        /// <param name="index">The index of the currently selected Photo in the given list.</param>
        public SingleImageViewModel(List<Photo> galleryItems, int index)
        {
            // Initialize commands
            _leftCommand = new RelayCommand(Left);
            _rightCommand = new RelayCommand(Right);

            _currentImage = new ImageViewModel(256, 0);


            GalleryItems = galleryItems;
            CurrentIndex = index;

            // Load the current image
            UpdateImage();
        }

        #endregion Constructors



        #region Fields and Properties

        /*
         * A collection of images that are viewable from this page. The user can cycle between these images.
         * TODO This can be outdated - if somehow the gallery contents change while viewing these images, this list will
         * not reflect those changes. That's something to fix in the future.
         */
        private List<Photo> GalleryItems;
        
        /*
         * The index of the currently selected image from the viewable list of images.
         */
        private int CurrentIndex;


        private ImageViewModel _currentImage;
        /// <summary>
        /// The ViewModel that holds the currently-selected image to display.
        /// </summary>
        public ImageViewModel CurrentImage
        {
            get { return _currentImage; }
        }

        /*
         * Updates the CurrentImage view model to represent the currently selected image from the collection.
         */
        private async void UpdateImage()
        {
            _currentImage.Photo = GalleryItems[CurrentIndex];
            await Task.Run(() => { _currentImage.UpdateImage(); });
        }


        /// <summary>
        /// Cancels any image loading operations that might be going on when this page is no longer on the top.
        /// </summary>
        public override void NavigatorLostFocus()
        {
            CurrentImage.CancelAllLoads();
        }


        #endregion Fields and Properties



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
                CurrentIndex = GalleryItems.Count - 1;

            UpdateImage();
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
            if (CurrentIndex >= GalleryItems.Count)
                CurrentIndex = 0;

            UpdateImage();
        }

        #endregion Commands
    }
}
