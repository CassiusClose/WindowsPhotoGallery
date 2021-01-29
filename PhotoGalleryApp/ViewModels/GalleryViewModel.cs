using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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

            // Init commands
            _openImageCommand = new RelayCommand(OpenImage);
            _addTagCommand = new RelayCommand(AddTag);
            _removeTagCommand = new RelayCommand(RemoveTag);


            _gallery = gallery;
            GalleryView = CollectionViewSource.GetDefaultView(_gallery);
            CurrentTags = new ObservableCollection<string>();

            GalleryView.Filter += ImageFilter;
            CurrentTags.CollectionChanged += CurrentTags_CollectionChanged;
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



        /// <summary>
        /// A collection of all tags in the associated gallery.
        /// </summary>
        public ObservableCollection<string> AllTags
        {
            get { return _gallery.Tags; }
        }

        /*
         * When the list of tags change, update the property here.
         */
        private void GalleryTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("AllTags");
            //TODO Remove any obsolete tags from CurrentTags
        }





        private ObservableCollection<string> _currentTags;
        /// <summary>
        /// A collection of the tags currently selected to be displayed.
        /// </summary>
        public ObservableCollection<string> CurrentTags
        {
            get { return _currentTags; }
            set
            {
                _currentTags = value;
                OnPropertyChanged();
                GalleryView.Refresh();
            }
        }

        /*
         * When the list of current tags changes, update the property.
         */
        private void CurrentTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("CurrentTags");
            GalleryView.Refresh();
        }


        #endregion Members



        #region Methods

        /// <summary>
        /// Filters the gallery's collection of images based on the selected tags.
        /// If no tags are selected, all images are accepted.
        /// </summary>
        /// <param name="item">The Models.Photo object to accept or reject.</param>
        /// <returns>bool, whether the photo was accepted or not.</returns>
        public bool ImageFilter(object item)
        {
            if (CurrentTags.Count == 0)
                return true;

            var image = item as Models.Photo;
            foreach (string tag in image.Tags)
            {
                bool found = false;
                foreach (string t in CurrentTags)
                {
                    if (t == tag)
                    {
                        found = true;
                        break;
                    }
                }
                if (found)
                    return true;
            }
            return false;
        }


        #endregion Methods



        #region Commands

        private RelayCommand _openImageCommand;
        /// <summary>
        /// A command which opens the specified Photo in a new page. Must pass the specified Photo as an argument.
        /// </summary>
        public ICommand OpenImageCommand => _openImageCommand;

        /// <summary>
        /// Opens the given Photo in a new page.
        /// </summary>
        /// <param name="parameter">The Models.Photo instance to open.</param>
        private void OpenImage(object parameter)
        {
            Photo p = parameter as Photo;
            List<Photo> list = GalleryView.OfType<Photo>().ToList();
            int index = 0;
            for (int i = 0; i < list.Count; i++)
                if (p == list[i])
                    index = i;

            ImageViewModel imagePage = new ImageViewModel(list, index);
            _navigator.NewPage(imagePage);
        }




        private RelayCommand _addTagCommand;
        /// <summary>
        /// A command which adds a tag to the list of selected tags.
        /// </summary>
        public ICommand AddTagCommand => _addTagCommand;

        /// <summary>
        /// Adds the given tag to the list of selected tags, if it is not already added.
        /// </summary>
        /// <param name="parameter">The tag to add, as a string.</param>
        public void AddTag(object parameter)
        {
            string tag = parameter as string;
            if (!CurrentTags.Contains(tag))
                CurrentTags.Add(tag);
        }



        private RelayCommand _removeTagCommand;
        /// <summary>
        /// A command which removes a tag from the list of selected tags.
        /// </summary>
        public ICommand RemoveTagCommand => _removeTagCommand;

        /// <summary>
        /// Removes the given tag from the list of selected tags.
        /// </summary>
        /// <param name="parameter">The tag to remove, as a string.</param>
        public void RemoveTag(object parameter)
        {
            CurrentTags.Remove(parameter as string);
        }


        #endregion Commands
    }
}
