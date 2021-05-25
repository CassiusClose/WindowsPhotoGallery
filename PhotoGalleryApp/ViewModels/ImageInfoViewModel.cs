using System;
using System.Collections.Generic;
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
    class ImageInfoViewModel : ViewModelBase
    {

        #region Constructors

        /// <summary>
        /// Initializes the VM to represent the given photo.
        /// </summary>
        /// <param name="photo">The Photo from which to show information.</param>
        public ImageInfoViewModel(Photo photo)
        {
            _addTagCommand = new RelayCommand(AddTag);
            _removeTagCommand = new RelayCommand(RemoveTag);

            _photo = photo;
            TagsView = CollectionViewSource.GetDefaultView(photo.Tags);
        }

        #endregion Constructors


        #region Fields and Properties

        // The photo that contains all the information
        private Photo _photo;


        /// <summary>
        /// The filepath that the image is located at.
        /// </summary>
        public string Path
        {
            get { return _photo.Path; }
        }




        // A view which stores all the photo's tags
        public ICollectionView TagsView { get; }



        private string _newTagInput;
        /// <summary>
        /// Contains the contents of the textbox in which the user will enter new tags for the photo.
        /// </summary>
        public string NewTagInput
        {
            get { return _newTagInput; }
            set
            {
                _newTagInput = value;
                OnPropertyChanged();
            }
        }

        #endregion Fields and Properties



        #region Commands

        private RelayCommand _addTagCommand;
        /// <summary>
        /// A command which attempts to add the contents of the NewTagInput as a new tag of the image. If the string is
        /// empty or already a tag, nothing will change.
        /// </summary>
        public ICommand AddTagCommand => _addTagCommand;

        /// <summary>
        /// Attempts to add the contents of the NewTagInput as a new tag of the photo. If the tag string is empty or already
        /// a tag of the photo, nothing will change.
        /// </summary>
        public void AddTag() {
            string tag = NewTagInput;
            if (tag == null)
                return;
            
            if (tag == "")
                return;

            // If the tag doesn't already exist
            if(!_photo.Tags.Contains(tag))
            {
                _photo.Tags.Add(tag);
            }
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
