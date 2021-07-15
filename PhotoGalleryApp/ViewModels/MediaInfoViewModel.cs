using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel which displays information about a certain Photo to the user.
    /// </summary>
    class MediaInfoViewModel : ViewModelBase
    {

        #region Constructors

        /// <summary>
        /// Initializes the VM to represent the given photo.
        /// </summary>
        /// <param name="photo">The Photo from which to show information.</param>
        public MediaInfoViewModel(Media photo, MediaGallery gallery)
        {
            //Init commands
            _removeTagCommand = new RelayCommand(RemoveTag);

            _photo = photo;
            TagsView = CollectionViewSource.GetDefaultView(photo.Tags);

            // Init the tag chooser VM. AddTag will be called whenever an item is selected or
            // created from the drop-down.
            _tagChooser = new ChooserDropDownViewModel(new CollectionViewSource { Source = gallery.Tags }.View, AddTag);
        }

        #endregion Constructors


        #region Fields and Properties

        // The photo that contains all the information
        private Media _photo;


        /// <summary>
        /// The filepath that the image is located at.
        /// </summary>
        public string Path
        {
            get { return _photo.Filepath; }
        }




        // A view which stores all the photo's tags
        public ICollectionView TagsView { get; }



        private ChooserDropDownViewModel _tagChooser;
        /// <summary>
        /// The ViewModel that hold info for the drop-down list where the user can add tags to the photo.
        /// </summary>
        public ChooserDropDownViewModel TagChooser
        {
            get { return _tagChooser; }
        }

        #endregion Fields and Properties


        #region Methods

        /// <summary>
        /// Adds the given tag to this VM's photo's list of tags. Will update the containing PhotoGallery's
        /// list of all tags.
        /// </summary>
        /// <param name="tag">The tag to be added.</param>
        /// <param name="isNew">False if the tag already exists in the gallery and true if it doesn't.</param>
        public void AddTag(string tag, bool isNew)
        {
            // If the tag doesn't already exist in the photo's tag list, add it
            if (isNew || !_photo.Tags.Contains(tag))
                _photo.Tags.Add(tag);
        }

        #endregion Methods



        #region Commands

        private RelayCommand _removeTagCommand;
        /// <summary>
        /// A command which attempts to remove the given tag from the photo. If the tag does not belong to the photo,
        /// nothing happens.
        /// </summary>
        public ICommand RemoveTagCommand => _removeTagCommand;

        /// <summary>
        /// Attempts to remove the given tag from the photo. If the tag does not belong to the photo, nothing happens.
        /// </summary>
        /// <param name="parameter">The string tag to remove from the photo.</param>
        public void RemoveTag(object parameter)
        {
            string tag = parameter as string;
            if (tag == null)
                return;

            _photo.Tags.Remove(tag);
        }

        #endregion Commands
    }
}
