using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Controls;
using System.Xml.Serialization;
using Microsoft.Win32;
using PhotoGalleryApp.Models;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for the PhotoGallery model, displays the photos to the user and lets users interact with them.
    /// </summary>
    class GalleryViewModel : ViewModelBase
    {
        #region Constructors
        public GalleryViewModel(NavigatorViewModel navigator, Models.PhotoGallery gallery)
        {
            _navigator = navigator;

            // Init commands
            _addFilesCommand = new RelayCommand(AddFiles);
            _selectImageCommand = new RelayCommand(SelectImage);
            _removeImagesCommand = new RelayCommand(RemoveImages, AreImagesSelected);
            _openImageCommand = new RelayCommand(OpenImage);
            _addTagCommand = new RelayCommand(AddTag);
            _removeTagCommand = new RelayCommand(RemoveTag);
            _saveGalleryCommand = new RelayCommand(SaveGallery);


            _gallery = gallery;
            GalleryView = CollectionViewSource.GetDefaultView(_gallery);
            CurrentTags = new ObservableCollection<string>();

            GalleryView.Filter += ImageFilter;
            CurrentTags.CollectionChanged += CurrentTags_CollectionChanged;
        }

        #endregion Constructors



        #region Fields and Properties

        // A reference to the navigator so we can add pages to it
        private NavigatorViewModel _navigator;


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


        #endregion Fields and Properties



        #region Methods

        /// <summary>
        /// Filters the gallery's collection of images based on the selected tags.
        /// If no tags are selected, all images are accepted.
        /// </summary>
        /// <param name="item">The Models.Photo object to accept or reject.</param>
        /// <returns>bool, whether the photo was accepted or not.</returns>
        public bool ImageFilter(object item)
        {
            // If no tags picked, show all of the images 
            if (CurrentTags.Count == 0)
                return true;

            var image = item as Models.Photo;
            // For each tag in the image
            foreach (string tag in image.Tags)
            {
                bool found = false;
                // See if the tag has been selected by the user
                foreach (string t in CurrentTags)
                {
                    if (t == tag)
                    {
                        found = true;
                        break;
                    }
                }
                // If it has, accept the image
                if (found)
                    return true;
            }
            return false;
        }


        #endregion Methods



        #region Commands


        private RelayCommand _addFilesCommand;
        /// <summary>
        /// A command that opens a dialog box and adds the selected image files to the gallery.
        /// </summary>
        public ICommand AddFilesCommand => _addFilesCommand;

        /// <summary>
        /// Opens a dialog box and adds the selected image files to the gallery.
        /// </summary>
        public void AddFiles()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
            if(fileDialog.ShowDialog() == true)
            {
                foreach (string filename in fileDialog.FileNames)
                {
                    Photo p = new Photo(filename);
                    _gallery.Add(p);
                }
            }
        }



        private RelayCommand _openImageCommand;
        /// <summary>
        /// A command which opens the specified Photo in a new page. Must pass the specified Photo as an argument.
        /// </summary>
        public ICommand OpenImageCommand => _openImageCommand;

        /// <summary>
        /// Opens the given Photo in a new page.
        /// </summary>
        /// <param name="parameter">The Models.Photo instance to open.</param>
        public void OpenImage(object parameter)
        {
            Photo p = parameter as Photo;
            List<Photo> list = GalleryView.OfType<Photo>().ToList();

            // Get photo's index in the above list
            int index = 0;
            for (int i = 0; i < list.Count; i++)
                if (p == list[i])
                    index = i;

            ImageViewModel imagePage = new ImageViewModel(list, index);
            _navigator.NewPage(imagePage);
        }



        private RelayCommand _saveGalleryCommand;
        /// <summary>
        /// A command which saves the gallery's state to disk.
        /// </summary>
        public ICommand SaveGalleryCommand => _saveGalleryCommand;

        /// <summary>
        /// Saves the gallery's state to disk.
        /// </summary>
        public void SaveGallery()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(PhotoGallery));
            TextWriter writer = new StreamWriter("gallery.xml");
            serializer.Serialize(writer, _gallery);
            writer.Close();
        }



        #region Selected Image Commands

        /*
         * Notifies all commands that deal with selected images that 
         * the selection has changed.
         */
        private void UpdateSelectedCommandsCanExecute()
        {
            _removeImagesCommand.InvokeCanExecuteChanged();
        }

        /*
         * A CanExecuteAction function for any command that deals with selected images.
         * The command will be allowed to execute if at least one image in the gallery
         * is selected.
         */
        private bool AreImagesSelected(object parameter)
        {
            System.Collections.IList list = parameter as System.Collections.IList;
            if (list.Count > 0)
                return true;
            return false;
        }



        private RelayCommand _selectImageCommand;
        /// <summary>
        /// A command which should be called when an image is selected.
        /// </summary>
        public ICommand SelectImageCommand => _selectImageCommand;

        /// <summary>
        /// Notifies the gallery that the selection of images has changed.
        /// </summary>
        public void SelectImage()
        {
            UpdateSelectedCommandsCanExecute();
        }



        private RelayCommand _removeImagesCommand;
        /// <summary>
        /// A command which removes the given Photos from the gallery.
        /// </summary>
        public ICommand RemoveImagesCommand => _removeImagesCommand;

        /// <summary>
        /// Removes the given images from the gallery.
        /// </summary>
        /// <param name="parameter">The list of images to remove, of type System.Windows.Controls.SelectedItemCollection (from ListBox SelectedItems).</param>
        public void RemoveImages(object parameter)
        {
            System.Collections.IList list = parameter as System.Collections.IList;
            List<Photo> photos = list.Cast<Photo>().ToList();
            foreach (Photo p in photos)
                _gallery.Remove(p);
        }


        #endregion Selected Image Commands



        #region Tag Commands

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

        #endregion Tag Commands

        #endregion Commands
    }
}
