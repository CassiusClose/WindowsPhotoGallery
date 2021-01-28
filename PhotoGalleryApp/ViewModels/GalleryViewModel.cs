using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for the PhotoGallery model, displays the photos to the user and lets users interact with them.
    /// </summary>
    class GalleryViewModel : ViewModelBase
    {
        #region Constructors

        // A reference to the navigator so we can add pages to it
        private NavigatorViewModel _navigator;

        public GalleryViewModel(NavigatorViewModel navigator, Models.PhotoGallery gallery)
        {
            _navigator = navigator;

            _openImageCommand = new RelayCommand(OpenImage);

            _gallery = gallery;
            GalleryView = CollectionViewSource.GetDefaultView(_gallery);
        }

        #endregion Constructors


        #region Members
        private Models.PhotoGallery _gallery;

        /// <summary>
        /// The gallery's current items (with any filtering & sorting applied)
        /// </summary>
        public ICollectionView GalleryView { get; }

        /// <summary>
        /// The Gallery's name.
        /// </summary>
        public string GalleryName
        {
            get { return _gallery.Name; }
            set
            {
                _gallery.Name = value;
                OnPropertyChanged();
            }
        }
        #endregion Members


        #region Commands

        private RelayCommand _openImageCommand;
        /// <summary>
        /// A Command which opens the specified Photo in a new page. Must pass the specified Photo as an argument.
        /// </summary>
        public ICommand OpenImageCommand
        {
            get { return _openImageCommand; }
        }

        /// <summary>
        /// Opens the given Photo in a new page.
        /// </summary>
        /// <param name="parameter">The given Models.Photo instance to open.</param>
        private void OpenImage(object parameter)
        {
            //Open image
        }

        #endregion Commands
    }
}
