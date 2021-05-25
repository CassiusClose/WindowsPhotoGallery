using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel which displays information about a certain Photo to the user.
    /// </summary>
    class ImageInfoViewModel : ViewModelBase
    {

        #region Constructors

        /// <summary>
        /// Initializes the VM to represent the given photo.
        /// </summary>
        /// <param name="photo">The Photo from which to show information.</param>
        public ImageInfoViewModel(Photo photo)
        {
            _photo = photo;
            TagsView = CollectionViewSource.GetDefaultView(photo.Tags);
        }

        #endregion Constructors


        #region Fields and Properties

        // The photo that contains all the information
        private Photo _photo;

        // A view which stores all the photo's tags
        public ICollectionView TagsView { get; }

        /// <summary>
        /// The filepath that the image is located at.
        /// </summary>
        public string Path
        {
            get { return _photo.Path; }
        }

        #endregion Fields and Properties
    }
}
