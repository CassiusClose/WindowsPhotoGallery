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
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PhotoGalleryApp.ViewModels
{
    /// <summary>
    /// A ViewModel for the PhotoGallery model, displays the photos to the user and lets users interact with them.
    /// </summary>
    class GalleryViewModel : ViewModelBase
    {
        #region Constructors
        public GalleryViewModel(NavigatorViewModel navigator, PhotoGallery gallery)
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
            _images = new ObservableCollection<ImageViewModel>();

            ImagesView = CollectionViewSource.GetDefaultView(_images);
            CurrentTags = new ObservableCollection<string>();


            ImagesView.Filter += ImageFilter;
            CurrentTags.CollectionChanged += CurrentTags_CollectionChanged;

            UpdateImages();
        }

        #endregion Constructors


      


        #region Fields and Properties

        // A reference to the navigator so we can add pages to it
        private NavigatorViewModel _navigator;

        // The collection of photos in the gallery
        private PhotoGallery _gallery;
        // The collection of images (image data) to display
        private ObservableCollection<ImageViewModel> _images;

        /// <summary>
        /// The gallery's current items (with any filtering & sorting applied)
        /// </summary>
        public ICollectionView ImagesView { get; }


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
                ImagesView.Refresh();
            }
        }

        /*
         * When the list of current tags changes, update the property.
         */
        private void CurrentTags_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged("CurrentTags");
            ImagesView.Refresh();
        }


        /// <summary>
        /// Stores the size of the gallery's thumbnails. Right now, this is a hard coded value. Later, it will
        /// be an adjustable slider.
        /// </summary>
        public int ThumbnailHeight
        {
            get { return 200; }
        }

        #endregion Fields and Properties



        #region Methods

        /**
         * Loads the thumbnails of all the images in the gallery asynchronously.
         * TODO Depending on how things go, this maybe should be changed so that
         * only the ones in view are being loaded at any given time.
         */
        private async void UpdateImages()
        {
            _images.Clear();

            // Initialize all the image vms first without loading the images. This way,
            // the whole gallery will be available and navigatable to in the slideshow
            // view.
            for (int i = 0; i < _gallery.Count; i++)
            {
                // 2nd argument is 0 so that it only loads the image once
                ImageViewModel vm = new ImageViewModel(_gallery[i], 0, ThumbnailHeight);
                _images.Add(vm);
            }
            
            // Now load the images one at a time
            for (int i = 0; i < _images.Count; i++)
            {
                await Task.Run(() => { _images[i].UpdateImage(); });
            }
        }



        /// <summary>
        /// Filters the gallery's collection of images based on the selected tags.
        /// If no tags are selected, all images are accepted.
        /// </summary>
        /// <param name="item">The Photo object to accept or reject.</param>
        /// <returns>bool, whether the photo was accepted or not.</returns>
        public bool ImageFilter(object item)
        {
            // If no tags picked, show all of the images 
            if (CurrentTags.Count == 0)
                return true;

            ImageViewModel ivm = item as ImageViewModel;
            Photo image = ivm.Photo;
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
        /// <param name="parameter">The Photo instance to open.</param>
        public void OpenImage(object parameter)
        {
            Photo p = parameter as Photo;
            
            // Get a list of all the currently visible images
            List<ImageViewModel> list = ImagesView.OfType<ImageViewModel>().ToList();
            List<Photo> photos = ImageViewModel.GetPhotoList(list);

            // Get the clicked on photo's index in the list of Photos
            int index = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (p == list[i].Photo)
                    index = i;
            }

            // Create a new page to view the clicked image
            ImageSlideshowViewModel imagePage = new ImageSlideshowViewModel(photos, index);
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
