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
using PhotoGalleryApp.Views;

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
            _gallery = gallery;
        }

        #endregion Constructors



        #region Fields and Properties

        // The photo & the containing gallery
        private Media _photo;
        private MediaGallery _gallery;


        /// <summary>
        /// The filepath that the image is located at.
        /// </summary>
        public string Path
        {
            get { return _photo.Filepath; }
        }


        /// <summary>
        /// A collection of the photo's tags
        /// </summary>
        public ObservableCollection<string> PhotoTags
        {
            get { return _photo.Tags; }
        }


        /// <summary>
        /// A collection of the containing gallery's tags
        /// </summary>
        public ObservableCollection<string> AllTags
        {
            get { return _gallery.Tags; }
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

        /// <summary>
        /// An event handler that adds the event's tag to this VM's Photo.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="args">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs</param>
        public void AddTagToPhoto(object sender, EventArgs args)
        {
            ItemChosenEventArgs itemArgs = (ItemChosenEventArgs)args;
            if (itemArgs.Item != null && !_photo.Tags.Contains(itemArgs.Item))
                _photo.Tags.Add(itemArgs.Item);
        }




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
