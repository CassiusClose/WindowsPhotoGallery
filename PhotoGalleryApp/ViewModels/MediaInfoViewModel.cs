using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
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
        /// Initializes the VM to represent the given media.
        /// </summary>
        /// <param name="photo">The Photo from which to show information.</param>
        public MediaInfoViewModel(Media photo, Models.MediaCollection gallery)
        {
            //Init commands
            _removeTagCommand = new RelayCommand(RemoveTag);

            _media = photo;
            _gallery = gallery;
        }

        #endregion Constructors



        #region Fields and Properties

        // The media & the containing gallery
        private Media _media;
        private Models.MediaCollection _gallery;


        /// <summary>
        /// The filepath that the media is located at.
        /// </summary>
        public string Path
        {
            get { return _media.Filepath; }
        }


        /// <summary>
        /// The DateTime that the media was created at.
        /// </summary>
        public DateTime Timestamp
        {
            get { return _media.Timestamp; }
        }


        /// <summary>
        /// A collection of the media's tags
        /// </summary>
        public ObservableCollection<string> PhotoTags
        {
            get { return _media.Tags; }
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
        /// Adds the given tag to this VM's media's list of tags. Will update the containing PhotoGallery's
        /// list of all tags.
        /// </summary>
        /// <param name="tag">The tag to be added.</param>
        /// <param name="isNew">False if the tag already exists in the gallery and true if it doesn't.</param>
        public void AddTag(string tag, bool isNew)
        {
            // If the tag doesn't already exist in the media's tag list, add it
            if (isNew || !_media.Tags.Contains(tag))
                _media.Tags.Add(tag);
        }

        #endregion Methods



        #region Commands

        /// <summary>
        /// An event handler that adds the event's tag to this VM's Media.
        /// </summary>
        /// <param name="sender">The element that this event was triggered on.</param>
        /// <param name="args">The event's arguments, of type PhotoGalleryApp.Views.ItemChosenEventArgs</param>
        public void AddTagToPhoto(object sender, EventArgs args)
        {
            ItemChosenEventArgs itemArgs = (ItemChosenEventArgs)args;
            if (itemArgs.Item != null && !_media.Tags.Contains(itemArgs.Item))
                _media.Tags.Add(itemArgs.Item);
        }




        private RelayCommand _removeTagCommand;
        /// <summary>
        /// A command which attempts to remove the given tag from the media. If the tag does not belong to the media,
        /// nothing happens.
        /// </summary>
        public ICommand RemoveTagCommand => _removeTagCommand;

        /// <summary>
        /// Attempts to remove the given tag from the media. If the tag does not belong to the media, nothing happens.
        /// </summary>
        /// <param name="parameter">The string tag to remove from the media.</param>
        public void RemoveTag(object parameter)
        {
            string tag = parameter as string;
            if (tag == null)
                return;

            _media.Tags.Remove(tag);
        }

        #endregion Commands
    }
}
