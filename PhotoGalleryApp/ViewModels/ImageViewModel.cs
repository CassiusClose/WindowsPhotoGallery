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
    /// A ViewModel that displays one image in the full window, can switch between images in the gallery.
    /// </summary>
    class ImageViewModel : ViewModelBase
    {
        #region Constructors
        public ImageViewModel(List<Photo> galleryItems, int index)
        {
            // Initialize commands
            _leftCommand = new RelayCommand(Left);
            _rightCommand = new RelayCommand(Right);

            GalleryItems = galleryItems;
            Console.WriteLine(galleryItems[index]);
            CurrentIndex = index;
        }

        #endregion Constructors



        #region Fields and Properties

        /// <summary>
        /// The filepath of the current image to display
        /// </summary>
        public string Path => GalleryItems[CurrentIndex].Path;

        /// <summary>
        /// How much the current image should be rotated.
        /// </summary>
        public Rotation Rotation => GalleryItems[CurrentIndex].Rotation;


        /*
         * List of images from the gallery that spawned this page. Let's the user cycle between images in the gallery.
         * This can be outdated - if somehow the gallery contents change while viewing these images, this list will
         * not reflect those changes. That's something to fix in the future.
         */
        private List<Photo> GalleryItems;
        
        /*
         * The index of the currently selected image from the list of images.
         */
        private int CurrentIndex;

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
        /// <param name="parameter">Unused command parameter.</param>
        public void Left()
        {
            CurrentIndex--;
            // Wrap to end
            if (CurrentIndex < 0)
                CurrentIndex = GalleryItems.Count - 1;

            OnPropertyChanged("Path");
            OnPropertyChanged("Rotation");
        }



        private RelayCommand _rightCommand;
        /// <summary>
        /// A command that switches the display to the image after (or to the right of) the current image.
        /// </summary>
        public ICommand RightCommand => _rightCommand;

        /// <summary>
        /// Switches the display to the image after (or to the right of) the current image.
        /// </summary>
        /// <param name="parameter">Unused command parameter.</param>
        public void Right()
        {
            CurrentIndex++;
            // Wrap to beginning
            if (CurrentIndex >= GalleryItems.Count)
                CurrentIndex = 0;

            OnPropertyChanged("Path");
            OnPropertyChanged("Rotation");
        }

        #endregion Commands
    }
}
